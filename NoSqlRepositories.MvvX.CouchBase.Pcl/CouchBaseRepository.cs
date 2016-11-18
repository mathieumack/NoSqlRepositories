using MvvmCross.Plugins.File;
using MvvX.Plugins.CouchBaseLite;
using MvvX.Plugins.CouchBaseLite.Database;
using MvvX.Plugins.CouchBaseLite.Documents;
using MvvX.Plugins.CouchBaseLite.Queries;
using Newtonsoft.Json.Linq;
using NoSqlRepositories.Core;
using NoSqlRepositories.Core.Helpers;
using NoSqlRepositories.Core.NoSQLException;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace NoSqlRepositories.MvvX.CouchBase.Pcl
{
    /// <summary>
    /// The repository can contains instance of subtype of the type T and handle polymorphism. For that
    /// the subclasses of class T must be declared in the attribute PolymorphicTypes
    /// Limitations : couchbase repository doesn't handle polymorphism in attribute's entity of type List, Dictionary...
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CouchBaseRepository<T> : RepositoryBase<T> where T : class, IBaseEntity
    {
        /// <summary>
        /// Contains list of subclasses of the class T to handle polymorphism during deserialization
        /// </summary>
        public IDictionary<string, Type> PolymorphicTypes { get; set; } = new Dictionary<string, Type>();
        private readonly ICouchBaseLite couchBaseLite;
        private readonly IMvxFileStore fileStore;

        private IDatabase database;
        public string ConnectionStr { get; set; }

        public CouchBaseRepository(ICouchBaseLite couchBaseLite, IMvxFileStore fileStore)
        {
            Contract.Requires<ArgumentNullException>(couchBaseLite != null);
            Contract.Requires<ArgumentNullException>(fileStore != null);

            this.couchBaseLite = couchBaseLite;
            this.fileStore = fileStore;
            this.ConnectionStr = "SKM/Viewer/Couchbase";
            this.TypeName = typeof(T).Name;

            ConnectToDatabase();
        }

        private void ConnectToDatabase()
        {
            var databaseOptions = this.couchBaseLite.CreateDatabaseOptions();
            databaseOptions.Create = true;
            databaseOptions.StorageType = MvvX.Plugins.CouchBaseLite.Storages.StorageTypes.Sqlite;
            fileStore.EnsureFolderExists(ConnectionStr);

            this.database = this.couchBaseLite.CreateConnection(fileStore.NativePath(ConnectionStr), TypeName, databaseOptions);

            if (this.database == null)
                throw new NullReferenceException("CreateConnection returned no connection");
        }

        public override T GetById(string id)
        {
            //JsonConvert.DeserializeObject
            var documentObjet = this.database.GetDocument(id);
            if (documentObjet == null || string.IsNullOrEmpty(documentObjet.CurrentRevisionId) || documentObjet.Deleted)
            {
                throw new KeyNotFoundNoSQLException();
            }

            T entity = getEntityFromDocument(documentObjet);
            return entity;
        }

        /// <summary>
        /// Extract en Entity stored in the Couchbase document
        /// </summary>
        /// <param name="documentObjet"></param>
        /// <returns></returns>
        private T getEntityFromDocument(IDocument documentObjet)
        {

            T entity = null;

            // Comprendre pourquoi T ou Jobject sont retournés alternativement
            if (documentObjet.GetProperty("members").GetType() == typeof(JObject))
            {

                JObject testobject = (JObject)documentObjet.GetProperty("members");

                // Determine the destination type to handle polymorphism
                Type destinationType = typeof(T);

                string originalEntityType = (string)documentObjet.GetProperty("entityType");
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
                entity = (T)documentObjet.GetProperty("members");

            }

            return entity;
        }

        public override T TryGetById(string id)
        {
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
            var documentObjet = this.database.GetDocument(id);
            return documentObjet != null;
        }

        public override BulkInsertResult<string> InsertMany(IEnumerable<T> entities, InsertMode insertMode)
        {
            var insertResult = new BulkInsertResult<string>();

            Parallel.ForEach(entities, (entity) =>
            {
                // Create the document
                InsertOne(entity, insertMode);
                insertResult[entity.Id] = InsertResult.unknown;
            });
            return insertResult;
        }

        public override InsertResult InsertOne(T entity, InsertMode insertMode)
        {
            var insertResult = default(InsertResult);
            bool documentAlreadyExists = false;
            IDocument documentObjet = null;

            var date = NoSQLRepoHelper.DateTimeUtcNow();
            IDictionary<string, object> properties;

            if (string.IsNullOrEmpty(entity.Id))
            {
                // No id specified, let couchbase generate the id and affect it to the entity
                documentObjet = database.CreateDocument();
                entity.Id = documentObjet.Id;
            }
            else
            {
                documentObjet = database.GetDocument(entity.Id);

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
            var updateResult = default(UpdateResult);

            if (updateMode == UpdateMode.db_implementation)
            {
                var idDocument = entity.Id;
                var updateDate = NoSQLRepoHelper.DateTimeUtcNow();

                var documentObjet = database.GetDocument(idDocument);

                documentObjet.Update((IUnsavedRevision newRevision) =>
                {
                    var properties = newRevision.Properties;
                    properties["update date"] = updateDate;
                    properties["members"] = entity;
                    return true;
                });
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
            throw new NotImplementedException();
        }

        public override long TruncateCollection()
        {
            var query = database.CreateAllDocumentsQuery();
            query.AllDocsMode = QueryAllDocsMode.AllDocs;
            var result = query.Run();
            foreach (var resultItem in result)
            {
                resultItem.Document.Delete();
            }
            return 0;
        }

        public override void DropCollection()
        {
            throw new NotImplementedException();
        }

        public override void SetCollectionName(string typeName)
        {
            this.TypeName = typeName;
        }

        public override void InitCollection()
        {
            throw new NotImplementedException();
        }

        public override bool CollectionExists(bool createIfNotExists)
        {
            throw new NotImplementedException();
        }

        public override long Delete(string id, bool physical)
        {
            if (!physical)
                throw new NotImplementedException();

            long result = 0;
            var documentObjet = this.database.GetDocument(id);
            // Document found
            if (documentObjet != null && !documentObjet.Deleted)
            {
                documentObjet.Delete();
                result = 1;
            }

            return result;
        }

        #region Attachments
        /// <summary>
        /// Add an attachment to an entity
        /// </summary>
        /// <param name="id">id of entity</param>
        /// <param name="filePathAttachment">file path of the file to attach</param>
        /// <param name="contentType">type of the file to attach</param>
        /// <param name="attachmentName">identify of the file to attach</param>
        public override void AddAttachment(string id, string filePathAttachment, string contentType, string attachmentName)
        {
            var existingEntity = this.database.GetDocument(id);
            if (existingEntity == null)
                throw new KeyNotFoundNoSQLException();

            IUnsavedRevision newRevision = existingEntity.CurrentRevision.CreateRevision();

            using (var fileStream = fileStore.OpenRead(filePathAttachment))
            {
                newRevision.SetAttachment(attachmentName, contentType, fileStream);
                newRevision.Save();
            }
        }

        /// <summary>
        /// Remove the attachment of a document
        /// </summary>
        /// <param name="id">id of entity</param>
        /// <param name="attachmentName">name of attachment to remove</param>
        public override void RemoveAttachment(string id, string attachmentName)
        {
            var existingEntity = this.database.GetDocument(id);
            if (existingEntity == null)
                throw new KeyNotFoundNoSQLException(string.Format("Entity '{0}' not found", id));

            if (!AttachmentExists(existingEntity.CurrentRevision, attachmentName))
                throw new AttachmentNotFoundNoSQLException(string.Format("Attachement {0} not found on Entity '{1}'", attachmentName, id));

            IUnsavedRevision newRevision = existingEntity.CurrentRevision.CreateRevision();
            newRevision.RemoveAttachment(attachmentName);
            newRevision.Save();
        }

        /// <summary>
        /// Get an attachment of an entity or null if the attachement is not found
        /// </summary>
        /// <param name="id">Id of document</param>
        /// <param name="attachmentName">Name of attachment</param>
        /// <returns></returns>
        public override Stream GetAttachment(string id, string attachmentName)
        {
            var attachement = GetAttachmentCore(id, attachmentName);
            if (attachement != null)
                return attachement.ContentStream;
            else
                throw new AttachmentNotFoundNoSQLException();
        }

        public IEnumerable<byte> GetAttachmentInMemory(string id, string attachmentName)
        {
            Contract.Requires<ArgumentException>(!string.IsNullOrWhiteSpace(id));
            Contract.Requires<ArgumentException>(!string.IsNullOrWhiteSpace(attachmentName));

            var attachement = GetAttachmentCore(id, attachmentName);
            if (attachement != null)
                return attachement.Content;
            else
                return null;
        }

        /// <summary>
        /// Indicate if an attachement exits
        /// Should not use "revision.GetAttachment" == null that always returned an object instance, event if the attachment doesn't exist 
        /// </summary>
        /// <param name="revision"></param>
        /// <param name="attachmentName"></param>
        /// <returns></returns>
        private bool AttachmentExists(IRevision revision, string attachmentName)
        {
            return revision.AttachmentNames.Any(a => attachmentName.Equals(a));
        }

        private IAttachment GetAttachmentCore(string id, string attachmentName)
        {
            var documentAttachment = this.database.GetDocument(id);
            if (documentAttachment == null)
                throw new KeyNotFoundNoSQLException();

            var revision = documentAttachment.CurrentRevision;

            if (!AttachmentExists(revision, attachmentName))
                return null;
            else
            {
                var attachment = revision.GetAttachment(attachmentName);
                return attachment;
            }
        }

        #endregion

        public override void InitCollection(List<Expression<Func<T, object>>> indexFieldSelectors)
        {
            throw new NotImplementedException();
        }

        public override IList<T> GetAll()
        {
            List<T> listDocumentsOfCouchBase = new List<T>();
            var query = database.CreateAllDocumentsQuery();
            query.AllDocsMode = QueryAllDocsMode.AllDocs;
            var rows = query.Run();

            foreach (var row in rows)
            {
                var doc = row.Document;

                if (doc != null && !doc.Deleted)
                {
                    T entity = getEntityFromDocument(doc);
                    listDocumentsOfCouchBase.Add(entity);
                }
            }

            return listDocumentsOfCouchBase;
        }

        public override IList<string> GetAttachmentNames(string id)
        {
            var documentAttachment = this.database.GetDocument(id);
            if (documentAttachment == null)
                throw new KeyNotFoundNoSQLException();

            var revision = documentAttachment.CurrentRevision;

            return revision.AttachmentNames.ToList();
        }
    }
}
