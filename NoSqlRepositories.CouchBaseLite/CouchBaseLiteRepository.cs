﻿using Couchbase.Lite;
using Newtonsoft.Json.Linq;
using NoSqlRepositories.Core;
using NoSqlRepositories.Core.Helpers;
using NoSqlRepositories.Core.NoSQLException;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using NoSqlRepositories.Core.Queries;
using Couchbase.Lite.Query;

namespace NoSqlRepositories.CouchBaseLite
{
    /// <summary>
    /// The repository can contains instance of subtype of the type T and handle polymorphism. For that
    /// the subclasses of class T must be declared in the attribute PolymorphicTypes
    /// Limitations : CouchBaseLite repository doesn't handle polymorphism in attribute's entity of type List, Dictionary...
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CouchBaseLiteRepository<T> : RepositoryBase<T> where T : class, IBaseEntity
    {
        public override string DatabaseName
        {
            get
            {
                return this.database.Name;
            }
        }

        public override NoSQLEngineType EngineType
        {
            get
            {
                return NoSQLEngineType.CouchBaseLiteLite;
            }
        }

        /// <summary>
        /// Contains list of subclasses of the class T to handle polymorphism during deserialization
        /// </summary>
        public IDictionary<string, Type> PolymorphicTypes { get; } = new Dictionary<string, Type>();

        protected Database database;

        /// <summary>
        /// Empty constructor used for SqlCipher constructor
        /// </summary>
        protected CouchBaseLiteRepository()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="couchBaseLiteLite"></param>
        /// <param name="fileStore"></param>
        public CouchBaseLiteRepository(string directoryPath, string dbName)
        {
            if (string.IsNullOrWhiteSpace(directoryPath))
                throw new ArgumentNullException(nameof(directoryPath));
            if (string.IsNullOrWhiteSpace(dbName))
                throw new ArgumentNullException(nameof(dbName));

            Construct(directoryPath, dbName);
        }
        
        private void Construct(string directoryPath, string dbName)
        {
            if (string.IsNullOrWhiteSpace(dbName))
                throw new ArgumentNullException(nameof(dbName));
            
            this.CollectionName = typeof(T).Name;

            ConnectToDatabase(directoryPath, dbName);

            CreateAllDocView();

            ConnectAgainToDatabase = () => Construct(directoryPath, dbName);
        }
        
        private void ConnectToDatabase(string directoryPath, string dbName)
        {
            if (Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);

            var databaseOptions = new DatabaseConfiguration();
            databaseOptions.Directory = directoryPath;

            this.database = new Database(dbName, databaseOptions);

            ConnectionOpened = true;
        }

        public override async Task Close()
        {
            this.database.Close();
            ConnectionOpened = false;
        }

        public override void ConnectAgain()
        {
            ConnectAgainToDatabase();
        }

        //void CreateQuery()
        //{
        //    CheckOpenedConnection();

        //    var query = this.database.CreateAllDocumentsQuery();
        //}

        public override bool CompactDatabase()
        {
            CheckOpenedConnection();
            this.database.Compact();
            return true;
        }

        public override void ExpireAt(string id, DateTime? dateLimit)
        {
            throw new NotImplementedException("TTL feature will be available only in v2.5. More informations : https://github.com/couchbase/couchbase-lite-net/issues/1129");
            //CheckOpenedConnection();

            //database.SetDocumentExpiration(id, dateLimit);
        }

        public override T GetById(string id)
        {
            CheckOpenedConnection();
                       
            var documentObjet = this.database.GetDocument(GetInternalCBLId(id));

            if (documentObjet == null)
                throw new KeyNotFoundNoSQLException();

            T entity = GetEntityFromDocument(documentObjet);

            if (entity.Deleted)
                throw new KeyNotFoundNoSQLException();

            return entity;
        }

        /// <summary>
        /// Get the entities that match given ids. The list is empty if no entities were found
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public override IList<T> GetByIds(IList<string> ids)
        {
            CheckOpenedConnection();

            var objects = new List<T>();
            foreach (string id in ids)
            {
                var obj = TryGetById(id);
                if (obj != null)
                    objects.Add(obj);
            }
            return objects;
        }

        /// <summary>
        /// Extract en Entity stored in the CouchBaseLite document
        /// </summary>
        /// <param name="documentObjet"></param>
        /// <returns></returns>
        private T GetEntityFromDocument(Document documentObjet)
        {
            CheckOpenedConnection();

            return GetEntityFromDocument(documentObjet.GetValue("members"), documentObjet.GetString("entityType"));
        }

        private T GetEntityFromDocument(object memberField, string originalEntityType)
        {
            CheckOpenedConnection();

            T entity = null;

            if (memberField is JObject)
            {
                JObject testobject = (JObject)memberField;

                // Determine the destination type to handle polymorphism
                Type destinationType = typeof(T);

                if (!string.IsNullOrEmpty(originalEntityType))
                {
                    // We stored the original entity type
                    Type mappedDestinationType;
                    if (PolymorphicTypes.TryGetValue(originalEntityType, out mappedDestinationType))
                    {
                        // We found a mapped destination type
                        destinationType = mappedDestinationType;

                    }
                }

                entity = (T)testobject.ToObject(destinationType);
            }
            else
            {
                entity = (T)memberField;

            }

            return entity;
        }

        public override T TryGetById(string id)
        {
            // Refactor to optimize this implementation
            try
            {
                return GetById(id);
            }
            catch (KeyNotFoundNoSQLException)
            {
                return null;
            }
        }

        public override bool Exist(string id)
        {
            CheckOpenedConnection();

            var documentObjet = this.database.GetDocument(GetInternalCBLId(id));
            return documentObjet != null;
        }

        public override BulkInsertResult<string> InsertMany(IEnumerable<T> entities, InsertMode insertMode)
        {
            CheckOpenedConnection();

            var insertResult = new BulkInsertResult<string>();

            database.InBatch(() =>
            {
                foreach (var entity in entities)
                {
                    // Create the document
                    InsertOne(entity, insertMode);
                    insertResult[entity.Id] = InsertResult.unknown;
                }
            });
            
            return insertResult;
        }

        public override InsertResult InsertOne(T entity, InsertMode insertMode)
        {
            CheckOpenedConnection();

            var insertResult = default(InsertResult);
            bool documentAlreadyExists = false;
            Document documentObjet = null;

            var date = NoSQLRepoHelper.DateTimeUtcNow();
            IDictionary<string, object> properties;

            if (string.IsNullOrEmpty(entity.Id))
            {
                // No id specified, let CouchBaseLite generate the id and affect it to the entity
                documentObjet = database.CreateDocument();
                entity.Id = cblGeneratedIdPrefix + documentObjet.Id; // NB: prefix the Id generated by CouchBaseLite to be able to distinguish it with a user provided id
            }
            else
            {
                // Get an existing document or return a new one if not exists
                documentObjet = database.GetDocument(GetInternalCBLId(entity.Id));

                if (documentObjet.CurrentRevisionId != null)
                {
                    // Document already exists
                    if (insertMode == InsertMode.error_if_key_exists)
                    {
                        throw new DupplicateKeyNoSQLException();
                    }
                    else if (insertMode == InsertMode.do_nothing_if_key_exists)
                    {
                        return InsertResult.not_affected;
                    }

                    documentAlreadyExists = true;
                }
            }

            if (!documentAlreadyExists)
            {
                if (AutoGeneratedEntityDate)
                {
                    entity.SystemCreationDate = date;
                    entity.SystemLastUpdateDate = date;
                }

                properties = new Dictionary<string, object>()
                {
                    {"creat date", date},
                    {"update date", date},
                    {"collection", this.CollectionName},
                    {"members", entity},
                    {"entityType", entity.GetType().Name} // Store the original actual object class to handle polymorphism 
                };

                insertResult = InsertResult.inserted;
            }
            else
            {
                properties = documentObjet.Properties;

                entity.SystemCreationDate = (DateTime)properties["creat date"];
                entity.SystemLastUpdateDate = date;

                properties["update date"] = entity.SystemLastUpdateDate;
                properties["members"] = entity;

                insertResult = InsertResult.updated;
            }

            documentObjet.PutProperties(properties);

            return insertResult;
        }

        public override UpdateResult Update(T entity, UpdateMode updateMode)
        {
            CheckOpenedConnection();

            if (entity.Id == null)
                throw new ArgumentException("Cannot update an entity with a null field value");

            var updateResult = default(UpdateResult);

            if (updateMode == UpdateMode.db_implementation)
            {
                var idDocument = GetInternalCBLId(entity.Id);
                var updateDate = NoSQLRepoHelper.DateTimeUtcNow();

                using (var documentObjet = database.GetDocument(idDocument))
                {
                    using (var mutableDocument = documentObjet.ToMutable())
                    {
                        mutableDocument.SetDate("update date", updateDate);
                        mutableDocument.SetValue("members", entity);

                        database.Save(mutableDocument);
                    }
                }
                updateResult = UpdateResult.updated;
            }
            else
            {
                throw new NotImplementedException();
            }
            return updateResult;
        }

        public override void UseDatabase(string dbName)
        {
            CheckOpenedConnection();

            throw new NotImplementedException();
        }

        public override long TruncateCollection()
        {
            CheckOpenedConnection();

            int deleted = 0;

            IView view = database.GetView(CollectionName);
            using (IQuery query = view.CreateQuery())
            {
                query.Prefetch = true;
                query.AllDocsMode = QueryAllDocsMode.AllDocs;
                query.IndexUpdateMode = IndexUpdateMode.Before;

                using (var queryEnum = query.Run())
                {
                    foreach (IQueryRow resultItem in queryEnum)
                    {
                        resultItem.Document.Delete();
                        deleted += 1;
                    }
                }
            }

            return deleted;
        }

        public override void DropCollection()
        {
            CheckOpenedConnection();

            throw new NotImplementedException();
        }

        public override void SetCollectionName(string typeName)
        {
            CheckOpenedConnection();

            this.CollectionName = typeName;
        }

        public override void InitCollection()
        {
            CheckOpenedConnection();

            // Nothing to do to initialize the collection
        }

        public override bool CollectionExists(bool createIfNotExists)
        {
            CheckOpenedConnection();

            throw new NotImplementedException();
        }

        public override long Delete(string id, bool physical)
        {
            CheckOpenedConnection();

            long result = 0;
            var document = database.GetDocument(GetInternalCBLId(id));

            // Document found
            if (document != null)
            {
                this.database.Delete(document);
                result = 1;
            }

            return result;
        }

        #region Blobs

        /// <summary>
        /// Add an attachment to an entity
        /// </summary>
        /// <param name="id">id of entity</param>
        /// <param name="filePathAttachment">file path of the file to attach</param>
        /// <param name="contentType">type of the file to attach</param>
        /// <param name="attachmentName">identify of the file to attach</param>
        public override void AddAttachment(string id, Stream fileStream, string contentType, string attachmentName)
        {
            CheckOpenedConnection();

            var existingEntity = this.database.GetDocument(GetInternalCBLId(id));
            if (existingEntity == null)
                throw new KeyNotFoundNoSQLException();

            using (var mutableDocument = existingEntity.ToMutable())
            {
                mutableDocument.SetBlob(attachmentName, new Blob(contentType, fileStream));
                database.Save(mutableDocument);
            }
        }

        /// <summary>
        /// Remove the attachment of a document
        /// </summary>
        /// <param name="id">id of entity</param>
        /// <param name="attachmentName">name of attachment to remove</param>
        public override void RemoveAttachment(string id, string attachmentName)
        {
            CheckOpenedConnection();

            using (var existingEntity = this.database.GetDocument(GetInternalCBLId(id)))
            {
                if (existingEntity == null)
                    throw new KeyNotFoundNoSQLException(string.Format("Entity '{0}' not found", id));

                var blob = existingEntity.GetBlob(attachmentName);
                if (blob == null)
                    throw new AttachmentNotFoundNoSQLException(string.Format("Attachement {0} not found on Entity '{1}'", attachmentName, id));

                // TODO : Check how to remove blob from document !!
                using (var mutableDocument = existingEntity.ToMutable())
                {
                    //mutableDocument.blo
                    //database.Delete()
                    //newRevision.Save();
                }
            }
        }

        /// <summary>
        /// Get an attachment of an entity or null if the attachement is not found
        /// </summary>
        /// <param name="id">Id of document</param>
        /// <param name="attachmentName">Name of attachment</param>
        /// <returns></returns>
        public override Stream GetAttachment(string id, string attachmentName)
        {
            CheckOpenedConnection();

            var attachement = GetAttachmentCore(id, attachmentName);
            if (attachement != null)
                return attachement.ContentStream;
            else
                throw new AttachmentNotFoundNoSQLException();
        }

        public IEnumerable<byte> GetAttachmentInMemory(string id, string attachmentName)
        {
            CheckOpenedConnection();

            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentNullException("id");

            if (string.IsNullOrWhiteSpace(attachmentName))
                throw new ArgumentNullException("attachmentName");

            var attachement = GetAttachmentCore(id, attachmentName);
            if (attachement != null)
                return attachement.Content;

            return new List<byte>();
        }

        private Blob GetAttachmentCore(string id, string attachmentName)
        {
            var document = this.database.GetDocument(GetInternalCBLId(id));
            if (document == null)
                throw new KeyNotFoundNoSQLException();

            return document.GetBlob(attachmentName);
        }

        #endregion

        public override void InitCollection(IList<Expression<Func<T, object>>> indexFieldSelectors)
        {
            CheckOpenedConnection();

            throw new NotImplementedException();
        }

        public override IEnumerable<T> DoQuery(NoSqlQuery<T> queryFilters)
        {
            CheckOpenedConnection();

            IView view = database.GetView(CollectionName);

            using (IQuery query = view.CreateQuery())
            {
                query.Prefetch = true;
                query.AllDocsMode = QueryAllDocsMode.AllDocs;
                query.IndexUpdateMode = IndexUpdateMode.Before;
                if (queryFilters.Skip != 0)
                    query.Skip = queryFilters.Skip;
                if (queryFilters.Limit != 0)
                    query.Limit = queryFilters.Limit;
                if (queryFilters.PostFilter != null)
                {
                    query.PostFilter = (row) =>
                    {
                        var item = GetEntityFromDocument(row.Document);
                        return queryFilters.PostFilter(item);
                    };
                }
                using (var queryEnum = query.Run())
                {
                    return queryEnum.Select(row => GetEntityFromDocument(row.Document));
                }
            }
        }

        public override IEnumerable<T> GetAll()
        {
            CheckOpenedConnection();

            IView view = database.GetView(CollectionName);

            using (IQuery query = view.CreateQuery())
            {
                query.Prefetch = true;
                query.AllDocsMode = QueryAllDocsMode.AllDocs;
                query.IndexUpdateMode = IndexUpdateMode.Before;

                using (var queryEnum = query.Run())
                {
                    return queryEnum.Where(row => !row.Document.Deleted)
                        .Select(row => GetEntityFromDocument(row.Document));
                }
            }
        }

        public override IEnumerable<string> GetAttachmentNames(string id)
        {
            CheckOpenedConnection();

            var document = this.database.GetDocument(GetInternalCBLId(id));
            if (document == null)
                throw new KeyNotFoundNoSQLException();

            var properties = document.Keys;
            return properties.Where(e => document.GetBlob(e) != null);
        }
        
        #region Views

        public override IEnumerable<T> GetByField<TField>(string fieldName, TField value)
        {
            CheckOpenedConnection();

            IView view = database.GetExistingView(CollectionName + "-" + fieldName);

            if (view == null)
                throw new IndexNotFoundNoSQLException(string.Format("An index must be created on the fieldName '{0}' before calling GetByField", fieldName));

            using (IQuery query = view.CreateQuery())
            {
                query.Prefetch = true;
                query.StartKey = value;
                query.EndKey = value;
                query.IndexUpdateMode = IndexUpdateMode.Before;

                using (var queryEnum = query.Run())
                {
                    return queryEnum.Where(row => !row.Document.Deleted)
                        .Select(doc => GetEntityFromDocument(doc.Document));
                }
            }
        }

        public override IEnumerable<T> GetByField<TField>(string fieldName, List<TField> values)
        {
            CheckOpenedConnection();

            return values.SelectMany(v => GetByField(fieldName, v))
                .GroupBy(e => e.Id)
                .Select(g => g.First()); // Remove duplicates entities
        }

        public override IEnumerable<string> GetKeyByField<TField>(string fieldName, TField value)
        {
            CheckOpenedConnection();

            IView view = database.GetExistingView(CollectionName + "-" + fieldName);

            if (view == null)
                throw new IndexNotFoundNoSQLException(string.Format("An index must be created on the fieldName '{0}' before calling GetByField", fieldName));

            using (IQuery query = view.CreateQuery())
            {
                query.Prefetch = false;
                query.StartKey = value;
                query.EndKey = value;

                using (var queryEnum = query.Run())
                {
                    return queryEnum.Select(doc => GetIdFromInternalCBLId(doc.DocumentId));
                }
            }
        }

        public override int Count()
        {
            CheckOpenedConnection();

            IView view = database.GetView(CollectionName);

            using (IQuery query = view.CreateQuery())
            {
                query.Prefetch = false;
                query.AllDocsMode = QueryAllDocsMode.AllDocs;

                using (var queryEnum = query.Run())
                {
                    return queryEnum.Where(row => !row.Document.Deleted)
                        .Select(row => GetEntityFromDocument(row.Document))
                        .Count();
                }
            }
        }

        public override IEnumerable<string> GetKeyByField<TField>(string fieldName, List<TField> values)
        {
            CheckOpenedConnection();

            return values.SelectMany(v => GetKeyByField(fieldName, v)).Distinct();
        }

        /// <summary>
        /// Create a view to get All document of the collection without scranning the whole database
        /// Reminder : in CouchBaseLite, there is no "collection", all objects belong to the same storage
        /// </summary>
        protected void CreateAllDocView()
        {
            IView view = database.GetExistingView(CollectionName);
            if (view == null)
            {
                view = database.GetView(CollectionName);
            }

            view.SetMap(
            (doc, emit) =>
            {
                if (!doc.Keys.Contains("collection") || !doc.Keys.Contains("members"))
                    return; // bad doc format, ignore it

                var collection = (string)doc["collection"];
                if (collection == null | !collection.Equals(CollectionName))
                    return; // doc type is not the one of the current collection

                emit(doc["_id"], doc["_id"]);
            }
        , "1");

        }

        public void CreateView<TField>(string fieldName, string version)
        {
            CheckOpenedConnection();

            var viewName = CollectionName + "-" + fieldName; // view name = collectionName-fieldName
            IView view = database.GetExistingView(viewName);

            if (view == null)
            {
                view = database.GetView(viewName);
            }

            view.SetMap(
                  (doc, emit) =>
                  {
                      if (!doc.Keys.Contains("collection") || !doc.Keys.Contains("members"))
                          return; // bad doc format, ignore it

                      var collection = (string)doc["collection"];
                      if (collection == null | !collection.Equals(CollectionName))
                          return; // doc type is not the one of the current collection

                      JObject jObj = (JObject)doc["members"];

                      string id = jObj.GetValue("Id").Value<string>();

                      JToken jToken = jObj.GetValue(fieldName);
                      if (jToken is JArray)
                      {
                          foreach (var arrayToken in (JArray)jToken)
                          {
                              TField fieldValue = arrayToken.Value<TField>();
                              emit(fieldValue, id);
                          }
                      }
                      else
                      {
                          TField fieldValue = jToken.Value<TField>();
                          emit(fieldValue, id);
                      }
                  }
              , version);
        }

        #endregion

        #region Private

        /// <summary>
        /// Return the internal Id used in CouchBaseLite lite to uniquely identity in the database objects of all collections
        /// Reminder : CBL id's must be unique for all objects (whatever their collection/type)
        /// </summary>
        /// <param name="entityId"></param>
        /// <returns></returns>
        private string GetInternalCBLId(string entityId)
        {
            string cblId;
            if (entityId.StartsWith(cblGeneratedIdPrefix))
                // Entity Id has been generated by CouchBaseLite, entityId = CouchBaseLiteliteId
                cblId = entityId.Substring(cblGeneratedIdPrefix.Length);
            else
                // Entity Id is user provided, add the suffix to the entityId to ensure Ic
                cblId = string.Concat(CollectionName, "-", entityId);
            return cblId;
        }


        private string GetIdFromInternalCBLId(string cblId)
        {
            string id;

            if (cblId.StartsWith(CollectionName + "-"))
                // Entity Id has been generated by CouchBaseLite, entityId = CouchBaseLiteliteId
                id = cblId.Substring(CollectionName.Length + 1);
            else
                // Entity Id is user provided, add the suffix to the entityId to ensure Ic
                id = cblGeneratedIdPrefix + cblId;
            return id;
        }

        private const string cblGeneratedIdPrefix = "$$CBL$$";

        #endregion
    }
}