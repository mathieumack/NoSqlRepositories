using Couchbase.Lite;
using Couchbase.Lite.Query;
using Linq2CouchBaseLiteExpression;
using Newtonsoft.Json.Linq;
using NoSqlRepositories.Core;
using NoSqlRepositories.Core.Helpers;
using NoSqlRepositories.Core.NoSQLException;
using NoSqlRepositories.Core.Queries;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

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
        /// <param name="directoryPath"></param>
        /// <param name="dbName"></param>
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

            var databaseOptions = new DatabaseConfiguration()
            {
                Directory = directoryPath
            };

            this.database = new Database(dbName, databaseOptions);

            ConnectionOpened = true;
        }

        public override async Task Close()
        {
            this.database.Close();
            ConnectionOpened = false;

            await Task.CompletedTask;
        }

        public override void ConnectAgain()
        {
            ConnectAgainToDatabase();
        }

        public override bool CompactDatabase()
        {
            CheckOpenedConnection();
            this.database.PerformMaintenance(MaintenanceType.Compact);
            return true;
        }

        public override void ExpireAt(string id, DateTime? dateLimit)
        {
            CheckOpenedConnection();

            database.SetDocumentExpiration(id, dateLimit);
        }

        public override T GetById(string id)
        {
            CheckOpenedConnection();

            using (var documentObjet = this.database.GetDocument(id))
            {
                if (documentObjet == null)
                    throw new KeyNotFoundNoSQLException();

                var result = GetEntityDomain(documentObjet);

                if (result.Deleted)
                    throw new KeyNotFoundNoSQLException();

                return result;
            }
        }

        private T GetEntityDomain(Document document)
        {
            var simpleDictionary = document.ToDictionary();
            var fields = ObjectToDictionaryHelper.ListOfFields<T>();
            var objectFields = new Dictionary<string, object>();
            foreach (var field in fields)
            {
                if (simpleDictionary.ContainsKey(field))
                    objectFields.Add(field, simpleDictionary[field]);
            }
            JObject obj = JObject.FromObject(objectFields);
            return obj.ToObject<T>();
        }

        private void SetDocument(T entity, MutableDocument mutableDocument)
        {
            var properties = ObjectToDictionaryHelper.ToDictionary(entity);
            foreach (var prop in properties)
            {
                if (prop.Value is int)
                    mutableDocument.SetInt(prop.Key, (int)prop.Value);
                else if (prop.Value is long)
                    mutableDocument.SetLong(prop.Key, (long)prop.Value);
                else if (prop.Value is bool)
                    mutableDocument.SetBoolean(prop.Key, (bool)prop.Value);
                else if (prop.Value is DateTimeOffset)
                {
                    if ((DateTimeOffset)prop.Value != default(DateTimeOffset))
                        mutableDocument.SetDate(prop.Key, (DateTimeOffset)prop.Value);
                }
                else if (prop.Value is double)
                    mutableDocument.SetDouble(prop.Key, (double)prop.Value);
                else if (prop.Value is float)
                    mutableDocument.SetFloat(prop.Key, (float)prop.Value);
                else if (prop.Value is string)
                    mutableDocument.SetString(prop.Key, (string)prop.Value);
                else
                    mutableDocument.SetValue(prop.Key, prop.Value);
            }
        }

        /// <summary>
        /// Get the entities that match given ids. The list is empty if no entities were found
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public override IEnumerable<T> GetByIds(IList<string> ids)
        {
            CheckOpenedConnection();

            return ids.Select(e => TryGetById(e)).Where(e => e != null);
        }

        public override T TryGetById(string id)
        {
            CheckOpenedConnection();

            using (var documentObjet = this.database.GetDocument(id))
            {
                if (documentObjet == null)
                    return null;

                var result = GetEntityDomain(documentObjet);

                if (result.Deleted)
                    return null;

                return result;
            }
        }

        public override bool Exist(string id)
        {
            CheckOpenedConnection();

            var documentObjet = this.database.GetDocument(id);
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

            var createdDate = NoSQLRepoHelper.DateTimeUtcNow();
            var updateddate = NoSQLRepoHelper.DateTimeUtcNow();

            MutableDocument mutabledocument;

            if (!string.IsNullOrEmpty(entity.Id))
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
                            createdDate = document.GetDate("SystemCreationDate");
                            database.Delete(document);
                            break;
                        case InsertMode.do_nothing_if_key_exists:
                            return InsertResult.not_affected;
                        default:
                            break;
                    }
                }

                mutabledocument = new MutableDocument(entity.Id);
            }
            else
                mutabledocument = new MutableDocument();

            // Normally, at this point, the document is deleted from database or an exception occured.
            // We can insert the new document :
            mutabledocument.SetString("collection", CollectionName);

            entity.SystemCreationDate = createdDate;
            entity.SystemLastUpdateDate = updateddate;
            entity.Id = mutabledocument.Id;

            SetDocument(entity, mutabledocument);

            database.Save(mutabledocument);

            return InsertResult.inserted;
        }

        public override UpdateResult Update(T entity, UpdateMode updateMode)
        {
            CheckOpenedConnection();

            if (updateMode != UpdateMode.db_implementation)
                throw new NotImplementedException();

            using (var document = database.GetDocument(entity.Id))
            {
                if (document == null)
                    throw new KeyNotFoundNoSQLException();

                using (var mutabledocument = document.ToMutable())
                {
                    // Update update date
                    var date = NoSQLRepoHelper.DateTimeUtcNow();
                    entity.SystemLastUpdateDate = date;

                    SetDocument(entity, mutabledocument);

                    database.Save(mutabledocument);
                    return UpdateResult.updated;
                }
            }
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

            foreach (var indexfield in indexFieldSelectors)
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
                if (physical)
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
        /// <param name="fileStream">file path of the file to attach</param>
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

                using (var mutableDocument = existingEntity.ToMutable())
                {
                    mutableDocument.SetBlob(attachmentName, null);
                    database.Save(mutableDocument);
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

        public override IEnumerable<T> DoQuery(NoSqlQuery<T> queryFilters)
        {
            CheckOpenedConnection();

            IExpression whereExpression = Expression.Property("collection").EqualTo(Expression.String(CollectionName));

            // Interpret Linq query to expression
            if (queryFilters.Filter != null)
            {
                var wherePreFilterExpression = Linq2CouchbaseLiteExpression.GenerateFromExpression(queryFilters.Filter);
                if (wherePreFilterExpression != null)
                    whereExpression = whereExpression.And(wherePreFilterExpression);
            }
            
            var queryBuilder = QueryBuilder.Select(SelectResult.Expression(Meta.ID))
                                            .From(DataSource.Database(database))
                                            .Where(whereExpression)
                                            // add default ordering by creation date :
                                            .OrderBy(Ordering.Property("SystemCreationDate").Ascending())
                                            .Limit(queryFilters.Limit > 0 ? Expression.Int(queryFilters.Limit + queryFilters.Skip) : Expression.Int(int.MaxValue));

            IList<string> ids = null;
            using (var query = queryBuilder)
            {
                ids = query.Execute().Skip(queryFilters.Skip).Select(row => row.GetString("id")).ToList();
            }

            var resultSet = ids.Select(e => GetById(e));

            return resultSet;
        }

        public override IEnumerable<T> GetAll()
        {
            CheckOpenedConnection();

            var ids = GetIds();

            return ids.Select(e => GetById(e));
        }

        public override AttachmentDetail GetAttachmentDetail(string id, string attachmentName)
        {
            CheckOpenedConnection();

            var attachment = GetAttachmentCore(id, attachmentName);
            if (attachment != null)
                return new AttachmentDetail()
                {
                    FileName = attachmentName,
                    ContentType = attachment.ContentType
                };
            else
                throw new AttachmentNotFoundNoSQLException();
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

        /// <inheritdoc/>
        public override IEnumerable<string> GetIds()
        {
            CheckOpenedConnection();

            using (var query = QueryBuilder.Select(SelectResult.Expression(Meta.ID))
                                .From(DataSource.Database(database))
                                .Where(Expression.Property("collection").EqualTo(Expression.String(CollectionName))))
            {
                return query.Execute().Select(row => row.GetString("id")).ToList();
            }
        }
    }
}