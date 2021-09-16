using LiteDB;
using Newtonsoft.Json;
using NoSqlRepositories.Core;
using NoSqlRepositories.Core.Helpers;
using NoSqlRepositories.Core.NoSQLException;
using NoSqlRepositories.Core.Queries;
using NoSqlRepositories.LiteDb.Helpers;
using NoSqlRepositories.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace NoSqlRepositories.LiteDb
{
    public class LiteDbRepository<T> : RepositoryBase<T> where T : class, IBaseEntity, new()
    {
        #region Members

        private string dbName;
        public override string DatabaseName
        {
            get
            {
                return dbName;
            }
        }

        public override NoSQLEngineType EngineType
        {
            get
            {
                return NoSQLEngineType.LiteDb;
            }
        }

        private string dbPath;
        private LiteDatabase localDb;
        private ILiteCollection<T> collection;
        private ILiteCollection<ExpirationEntry> expiredCollection;

        #endregion

        public LiteDbRepository(string directoryPath, string dbName)
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

            this.dbName = dbName;
            this.CollectionName = typeof(T).Name;

            ConnectToDatabase(directoryPath, dbName);

            ConnectAgainToDatabase = () => Construct(directoryPath, dbName);
        }


        private void ConnectToDatabase(string directoryPath, string dbName)
        {
            if (Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);

            dbPath = Path.Combine(directoryPath, dbName + ".db");
            localDb = new LiteDatabase(dbPath);
            collection = localDb.GetCollection<T>();
            collection.EnsureIndex("Id");
            collection.EnsureIndex("Deleted");

            expiredCollection = localDb.GetCollection<ExpirationEntry>();
            expiredCollection.EnsureIndex("Id");
            expiredCollection.EnsureIndex("ExpirationDate");

            ConnectionOpened = true;
        }

        #region INoSQLRepository

        public override Task Close()
        {
            if (localDb != null)
                localDb.Dispose();

            ConnectionOpened = false;

            return Task.CompletedTask;
        }

        public override void ConnectAgain()
        {
            ConnectAgainToDatabase();
        }

        public override T GetById(string id)
        {
            CheckOpenedConnection();

            T elt = collection.FindOne(e => !e.Deleted && e.Id.Equals(id));

            if (elt == null)
            {
                throw new KeyNotFoundNoSQLException(string.Format("Id '{0}' not found in the repository '{1}'", id, dbPath));
            }

            if (elt.Deleted)
            {
                throw new KeyNotFoundNoSQLException(string.Format("Id '{0}' not found in the repository '{1}'", id, dbPath));
            }

            if (IsExpired(id))
            {
                throw new KeyNotFoundNoSQLException(string.Format("Id '{0}' not found in the repository '{1}'", id, dbPath));
            }

            return elt;
        }

        private bool IsExpired(string id)
        {
            var entity = expiredCollection.FindOne(e => e.Id == id);
            return entity != null && entity.ExpirationDate.HasValue && entity.ExpirationDate >= DateTime.UtcNow;
        }

        public override IEnumerable<T> GetByIds(IList<string> ids)
        {
            CheckOpenedConnection();

            return ids.Select(e => TryGetById(e)).Where(e => e != null);
        }

        public override InsertResult InsertOne(T entity, InsertMode insertMode)
        {
            CheckOpenedConnection();

            var entitydomain = entity;

            NoSQLRepoHelper.SetIds(entitydomain);

            var updateddate = NoSQLRepoHelper.DateTimeUtcNow();
            var createdDate = NoSQLRepoHelper.DateTimeUtcNow();

            if (!string.IsNullOrEmpty(entity.Id) && Exist(entity.Id))
            {
                // Document already exists
                switch (insertMode)
                {
                    case InsertMode.error_if_key_exists:
                        throw new DupplicateKeyNoSQLException();
                    case InsertMode.erase_existing:
                        createdDate = GetById(entity.Id).SystemCreationDate;
                        Delete(entity.Id);
                        break;
                    case InsertMode.do_nothing_if_key_exists:
                        return InsertResult.not_affected;
                    default:
                        break;
                }
            }

            if (AutoGeneratedEntityDate)
            {
                entitydomain.SystemCreationDate = createdDate;
                entitydomain.SystemLastUpdateDate = updateddate;
            }

            // Clone to not shared reference between App instance and repository persisted value
            // UTC : Ensure to store only utc datetime
            var entityToStore = NewtonJsonHelper.CloneJson(entitydomain, DateTimeZoneHandling.Utc);

            collection.Insert(entityToStore);

            ExpireAt(entity.Id, null);

            return InsertResult.inserted;
        }

        public override BulkInsertResult<string> InsertMany(IEnumerable<T> entities, InsertMode insertMode)
        {
            CheckOpenedConnection();

            if (insertMode != InsertMode.db_implementation)
                throw new NotImplementedException();

            var insertResult = new BulkInsertResult<string>();

            foreach (var entity in entities)
            {
                var insertOneResult = InsertOne(entity, insertMode);
                // Normally the entity id has changed
                insertResult[entity.Id] = insertOneResult;
            }

            return insertResult;
        }

        public override bool Exist(string id)
        {
            CheckOpenedConnection();

            try
            {
                GetById(id);
                return true;
            }
            catch(Exception ex)
            {
                return false;
            }
        }

        public override UpdateResult Update(T entity, UpdateMode updateMode)
        {
            CheckOpenedConnection();

            if (updateMode == UpdateMode.upsert_if_missing_key)
                throw new NotImplementedException();

            // Update update date
            var date = NoSQLRepoHelper.DateTimeUtcNow();
            var entityToStore = entity;

            entityToStore.SystemLastUpdateDate = date;

            if (!Exist(entity.Id))
            {
                if (updateMode == UpdateMode.error_if_missing_key)
                    throw new KeyNotFoundNoSQLException("Misssing key '" + entity.Id + "'");
                else if (updateMode == UpdateMode.do_nothing_if_missing_key)
                    return UpdateResult.not_affected;
            }

            collection.Update(entity);

            ExpireAt(entity.Id, null);

            return UpdateResult.updated;
        }

        public override long Delete(string id, bool physical)
        {
            CheckOpenedConnection();

            if (Exist(id))
            {
                foreach (var attachmentName in GetAttachmentNames(id))
                {
                    RemoveAttachment(id, attachmentName);
                }

                if (physical)
                {
                    expiredCollection.Delete(new BsonValue(id));
                    if (collection.Delete(new BsonValue(id)))
                        return 1;
                    return 0;
                }
                else
                {
                    var entity = GetById(id);
                    entity.Deleted = true;
                    collection.Update(entity);
                }
                return 1;
            }
            else
            {
                return 0;
            }
        }

        public override T TryGetById(string id)
        {
            CheckOpenedConnection();

            try
            {
                return GetById(id);
            }
            catch (KeyNotFoundNoSQLException)
            {
                return null;
            }
        }

        public override int Count()
        {
            CheckOpenedConnection();

            if (this.localDb != null)
            {
                return collection.Count(e => !e.Deleted);
            }

            return 0;
        }

        public override void InitCollection(IList<string> indexFieldSelectors)
        {
            // Nothing to do to initialize the collection
        }

        #endregion

        #region INoSQLDB

        public override void ExpireAt(string id, DateTime? dateLimit)
        {
            CheckOpenedConnection();

            var entity = expiredCollection.FindOne(e => e.Id == id);
            if (entity == null)
                expiredCollection.Insert(new ExpirationEntry()
                {
                    Id = id,
                    ExpirationDate = dateLimit
                });
            else
            {
                entity.ExpirationDate = dateLimit;
                expiredCollection.Update(entity);
            }
        }

        public override bool CompactDatabase()
        {
            CheckOpenedConnection();

            if (this.localDb != null)
            {
                // Delete expired items
                var expiredEntities = expiredCollection.Find(e => e.ExpirationDate <= DateTime.UtcNow);
                foreach (var entity in expiredEntities)
                {
                    Delete(entity.Id, true);
                }

                // Delete all deleted items
                var deleteds = collection.Find(e => e.Deleted);
                foreach(var deleted in deleteds)
                {
                    Delete(deleted.Id, true);
                }
                localDb.Rebuild();
            }
            return true;
        }

        public override long TruncateCollection()
        {
            CheckOpenedConnection();

            if (this.localDb != null)
            {
                var count = localDb.GetCollection<T>(CollectionName).Count();
                localDb.DropCollection(CollectionName);

                // Remove also all attachments :


                return (long)count;
            }

            return 0;
        }

        public override void SetCollectionName(string typeName)
        {
            CheckOpenedConnection();

            CollectionName = typeName;
        }

        public override void InitCollection()
        {
            CheckOpenedConnection();

            // Autoinit, nothing to do
        }

        public override void UseDatabase(string dbName)
        {
            CheckOpenedConnection();

            throw new NotImplementedException();
        }

        public override bool CollectionExists(bool createIfNotExists)
        {
            CheckOpenedConnection();

            return true;
        }

        public override void DropCollection()
        {
            CheckOpenedConnection();

            localDb.DropCollection(CollectionName);
        }

        #endregion

        #region Attachments

        public override void AddAttachment(string id, Stream fileStream, string contentType, string attachmentName)
        {
            CheckOpenedConnection();

            string fileIdentifier = id + "_" + CollectionName + "_" + attachmentName;
            localDb.FileStorage.Upload(fileIdentifier, attachmentName, fileStream);
        }

        public override void RemoveAttachment(string id, string attachmentName)
        {
            CheckOpenedConnection();

            string fileIdentifier = id + "_" + CollectionName + "_" + attachmentName;

            var fileInfo = localDb.FileStorage.FindById(fileIdentifier);
            if (fileInfo == null)
                throw new AttachmentNotFoundNoSQLException();

            localDb.FileStorage.Delete(fileIdentifier);
        }

        public override Stream GetAttachment(string id, string attachmentName)
        {
            CheckOpenedConnection();

            string fileIdentifier = $"$/{CollectionName}/{id}/{attachmentName}";

            var fileInfo = localDb.FileStorage.FindById(fileIdentifier);
            if(fileInfo == null)
                throw new AttachmentNotFoundNoSQLException();

            return fileInfo.OpenRead();
        }

        /// <summary>
        /// Return all entities of repository
        /// </summary>
        /// <returns></returns>
        public override IEnumerable<T> GetAll()
        {
            CheckOpenedConnection();

            if (this.localDb != null)
            {
                return collection.Find(e => !e.Deleted && !IsExpired(e.Id));
            }

            return new List<T>();
        }

        /// <summary>
        /// Return the list of name of all attachements of a given entity
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public override IEnumerable<string> GetAttachmentNames(string id)
        {
            CheckOpenedConnection();

            string fileNamePrefix = $"$/{CollectionName}/{id}";

            return localDb.FileStorage.Find(fileNamePrefix)
                                        .Select(e => e.Id.Replace(fileNamePrefix, ""));
        }

        public override IEnumerable<string> GetKeyByField<TField>(string fieldName, List<TField> values)
        {
            // We need to implement an index to do it.
            throw new NotImplementedException();
        }

        public override IEnumerable<string> GetKeyByField<TField>(string fieldName, TField value)
        {
            // We need to implement an index to do it.
            throw new NotImplementedException();
        }

        #endregion

        #region Queries

        public override IEnumerable<T> DoQuery(NoSqlQuery<T> queryFilters)
        {
            throw new NotImplementedException("This method is obsolete. Please use the Query() call instead");
        }

        public override AttachmentDetail GetAttachmentDetail(string id, string attachmentName)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<string> GetIds()
        {
            CheckOpenedConnection();

            if (this.localDb != null)
            {
                return collection.Find(e => !e.Deleted && !IsExpired(e.Id))
                    .Select(e => e.Id);
            }

            return new List<string>();
        }

        public override INoSqlQueryable<T> Query()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}