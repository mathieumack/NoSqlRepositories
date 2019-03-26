using Couchbase.Lite;
using NoSqlRepositories.Core;
using NoSqlRepositories.Core.Helpers;
using NoSqlRepositories.Core.NoSQLException;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NoSqlRepositories.Core.Queries;
using Couchbase.Lite.Query;
using NoSqlRepositories.Core.interfaces;

namespace NoSqlRepositories.CouchBaseLite
{
    /// <summary>
    /// The repository can contains instance of subtype of the type T and handle polymorphism. For that
    /// the subclasses of class T must be declared in the attribute PolymorphicTypes
    /// Limitations : CouchBaseLite repository doesn't handle polymorphism in attribute's entity of type List, Dictionary...
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CouchBaseLiteRepository<T> : RepositoryBase<T> where T : class, IBaseEntity, new()
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

        public override INoSqlEntity<T> GetById(string id)
        {
            CheckOpenedConnection();
                       
            var documentObjet = this.database.GetDocument(id);

            if (documentObjet == null)
                throw new KeyNotFoundNoSQLException();

            var result = new NoSqlEntity<T>(documentObjet);

            if (result.GetBoolean("Deleted"))
                throw new KeyNotFoundNoSQLException();

            return result;
        }

        /// <summary>
        /// Get the entities that match given ids. The list is empty if no entities were found
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public override IEnumerable<INoSqlEntity<T>> GetByIds(IList<string> ids)
        {
            CheckOpenedConnection();

            return ids.Select(e => TryGetById(e)).Where(e => e != null);
        }

        public override INoSqlEntity<T> TryGetById(string id)
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

            var documentObjet = this.database.GetDocument(id);
            return documentObjet != null;
        }

        public override BulkInsertResult<string> InsertMany(IEnumerable<INoSqlEntity<T>> entities, InsertMode insertMode)
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

        /// <summary>
        /// Create a new empty document
        /// The document is no inserted in database
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public override INoSqlEntity<T> CreateNewDocument(T entity)
        {
            MutableDocument mutableDocument = null;
            if (string.IsNullOrWhiteSpace(entity.Id))
                mutableDocument = new MutableDocument();
            else
                mutableDocument = new MutableDocument(entity.Id);

            var noSqlEntity = new NoSqlEntity<T>(this.CollectionName, mutableDocument);
            noSqlEntity.SetEntityDomain(entity);
            return noSqlEntity;
        }

        /// <summary>
        /// Create a new empty document
        /// The document is no inserted in database
        /// </summary>
        /// <param name="id"></param>
        /// <param name="insertMode"></param>
        /// <returns></returns>
        public override INoSqlEntity<T> CreateNewDocument(string id)
        {
            var entity = new NoSqlEntity<T>(this.CollectionName, new MutableDocument(cblGeneratedIdPrefix + id));
            return entity;
        }

        /// <summary>
        /// Create a new empty document
        /// The document is no inserted in database
        /// </summary>
        /// <param name="id"></param>
        /// <param name="insertMode"></param>
        /// <returns></returns>
        public override INoSqlEntity<T> CreateNewDocument()
        {
            return new NoSqlEntity<T>(this.CollectionName, new MutableDocument());
        }

        public override InsertResult InsertOne(INoSqlEntity<T> entity, InsertMode insertMode)
        {
            CheckOpenedConnection();

            var nosqlEntity = entity as NoSqlEntity<T>;
            if (nosqlEntity == null || nosqlEntity.CollectionName != this.CollectionName)
                throw new InvalidOperationException("the entity was created from an other repository");

            var createdDate = NoSQLRepoHelper.DateTimeUtcNow();
            var updateddate = NoSQLRepoHelper.DateTimeUtcNow();

            if (!string.IsNullOrEmpty(nosqlEntity.Id))
            {
                // Get an existing document or return a new one if not exists
                var document = database.GetDocument(entity.Id);
                if (document != null)
                {
                    // Document already exists
                    switch (insertMode)
                    {
                        case InsertMode.error_if_key_exists:
                            throw new DupplicateKeyNoSQLException();
                        case InsertMode.erase_existing:
                            createdDate = document.GetDate("SystemCreationDate").Date;
                            database.Delete(document);
                            break;
                        case InsertMode.do_nothing_if_key_exists:
                            return InsertResult.not_affected;
                        default:
                            break;
                    }
                }
            }

            // Normally, at this point, the document is deleted from database or an exception occured.
            // We can insert the new document :
            
            nosqlEntity.SystemCreationDate = createdDate;
            nosqlEntity.SystemLastUpdateDate = updateddate;
            
            database.Save(nosqlEntity.Document.MutableDocument);

            nosqlEntity.Id = nosqlEntity.Document.MutableDocument.Id;

            return InsertResult.inserted;
        }

        public override UpdateResult Update(INoSqlEntity<T> entity, UpdateMode updateMode)
        {
            CheckOpenedConnection();

            var nosqlEntity = entity as NoSqlEntity<T>;
            if (nosqlEntity == null || nosqlEntity.CollectionName != this.CollectionName)
                throw new InvalidOperationException("the entity was created from an other repository");

            if (updateMode != UpdateMode.db_implementation)
                throw new NotImplementedException();

            // Update update date
            var date = NoSQLRepoHelper.DateTimeUtcNow();
            nosqlEntity.SetDate("SystemLastUpdateDate", date);


            database.Save(nosqlEntity.Document.MutableDocument);
            return UpdateResult.updated;
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

            var documents = new List<Document>();

            using (var query = QueryBuilder.Select(SelectResult.Expression(Meta.ID))
                                .From(DataSource.Database(database))
                                .Where(Expression.Property("collection").EqualTo(Expression.String(CollectionName))))
            {
                foreach (var result in query.Execute())
                {
                    documents.Add(database.GetDocument(result.GetString("id")));
                }
            }

            database.InBatch(() =>
            {
                foreach (var entity in documents)
                {
                    database.Purge(entity);
                    deleted++;
                }
            });

            return deleted;
        }

        public override void DropCollection()
        {
            CheckOpenedConnection();
            TruncateCollection();
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

        public override void InitCollection(IList<string> indexFieldSelectors)
        {
            if (indexFieldSelectors == null)
                throw new ArgumentNullException(nameof(indexFieldSelectors));

            CheckOpenedConnection();

            foreach(var indexfield in indexFieldSelectors)
            {
                CreateView(indexfield, indexfield);
            }
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
            var document = database.GetDocument(id);

            // Document found
            if (document != null)
            {
                if(physical)
                    this.database.Purge(document);
                else
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

            var existingEntity = this.database.GetDocument(id);
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

            using (var existingEntity = this.database.GetDocument(id))
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
            var document = this.database.GetDocument(id);
            if (document == null)
                throw new KeyNotFoundNoSQLException();

            return document.GetBlob(attachmentName);
        }

        #endregion

        public IEnumerable<INoSqlEntity<T>> DoQuery(NoSqlQuery<T> queryFilters)
        {
            // TODO : Review this method. Change documents query results ?
            CheckOpenedConnection();

            if (queryFilters.Skip != 0)
            {
                using (var query = QueryBuilder.Select(SelectResult.Expression(Meta.ID))
                                        .From(DataSource.Database(database))
                                        .Where(Expression.Property("collection").EqualTo(Expression.String(CollectionName)))
                                        .OrderBy(Ordering.Property("title").Ascending())
                                        .Limit(Expression.Int(queryFilters.Skip)))
                {
                    return query.Execute().Select(row => GetById(row.GetString("id")));
                }
            }

            if (queryFilters.Skip != 0)
            {
                using (var query = QueryBuilder.Select(SelectResult.Expression(Meta.ID))
                                        .From(DataSource.Database(database))
                                        .Where(Expression.Property("collection").EqualTo(Expression.String(CollectionName))))
                {
                    return query.Execute().Select(row => GetById(row.GetString("id")));
                }
            }

            return new List<INoSqlEntity<T>>();
        }

        public override IEnumerable<INoSqlEntity<T>> GetAll()
        {
            CheckOpenedConnection();

            IList<string> ids = null;

            using (var query = QueryBuilder.Select(SelectResult.Expression(Meta.ID))
                                .From(DataSource.Database(database))
                                .Where(Expression.Property("collection").EqualTo(Expression.String(CollectionName))))
            {
                ids = query.Execute().Select(row => row.GetString("id")).ToList();
            }

            return ids.Select(e => GetById(e));
        }

        public override IEnumerable<string> GetAttachmentNames(string id)
        {
            CheckOpenedConnection();

            var document = this.database.GetDocument(id);
            if (document == null)
                throw new KeyNotFoundNoSQLException();

            var properties = document.Keys;
            return properties.Where(e => document.GetBlob(e) != null);
        }
        
        #region Views

        public override int Count()
        {
            CheckOpenedConnection();

            using (var query = QueryBuilder.Select(SelectResult.Expression(Meta.ID))
                                .From(DataSource.Database(database))
                                .Where(Expression.Property("collection").EqualTo(Expression.String(CollectionName))))
            {
                return query.Execute().Count();
            }
        }

        public override IEnumerable<string> GetKeyByField<TField>(string fieldName, TField value)
        {
            CheckOpenedConnection();

            using (var query = QueryBuilder.Select(
                                        SelectResult.Expression(Meta.ID),
                                        SelectResult.Property(fieldName))
                                    .From(DataSource.Database(database))
                                    .Where(Expression.Property(fieldName).EqualTo(Expression.Value(value))))
            {
                return query.Execute().Select(e => e.GetString("id"));
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
            CreateView("collection", "allDocView");
        }

        public void CreateView(string fieldName, string indexName)
        {
            CheckOpenedConnection();

            var viewName = CollectionName + "-" + fieldName; // view name = collectionName-fieldName
            database.CreateIndex(viewName, IndexBuilder.ValueIndex(items: ValueIndexItem.Property(fieldName)));
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

        private const string cblGeneratedIdPrefix = "$$CBL$$";

        #endregion
    }
}