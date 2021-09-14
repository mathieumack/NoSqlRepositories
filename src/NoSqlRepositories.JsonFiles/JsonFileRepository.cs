﻿using Newtonsoft.Json;
using NoSqlRepositories.Core;
using NoSqlRepositories.Core.Helpers;
using NoSqlRepositories.Core.NoSQLException;
using NoSqlRepositories.Core.Queries;
using NoSqlRepositories.JsonFiles.Helpers;
using NoSqlRepositories.JsonFiles.Queries;
using NoSqlRepositories.Shared;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace NoSqlRepositories.JsonFiles
{
    public class JsonFileRepository<T> : RepositoryBase<T> where T : class, IBaseEntity, new()
    {
        #region Members

        private readonly string dbName;
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
                return NoSQLEngineType.JsonFile;
            }
        }

        private readonly string dbDirectoryPath;

        private string AttachmentsDirectoryPath
        {
            get
            {
                return dbDirectoryPath + "/Attachments/" + CollectionName;
            }
        }

        /// <summary>
        /// File path taht contains attachment details
        /// </summary>
        public string AttachmentsFilePath
        {
            get
            {
                return dbDirectoryPath + "/" + CollectionName + ".Attachments.json";
            }
        }

        public string DbFilePath
        {
            get
            {
                return dbDirectoryPath + "/" + CollectionName + ".json";
            }
        }

        public string DbConfigFilePath
        {
            get
            {
                return dbDirectoryPath + "/" + CollectionName + ".config.json";
            }
        }

        internal IDictionary<string, T> LocalDb 
        {
            get
            {
                return localDb;
            }
        }

        private IDictionary<string, T> localDb;
        private IDictionary<string, AttachmentDetail> attachmentDetails;
        private DbConfiguration config;

        #endregion

        public JsonFileRepository(string dbDirectoryPath, string dbName)
        {
            if (string.IsNullOrWhiteSpace(dbDirectoryPath))
                throw new ArgumentNullException("dbDirectoryPath");

            this.dbDirectoryPath = Path.Combine(dbDirectoryPath, dbName);

            this.dbName = dbName;
            CollectionName = typeof(T).Name;
            LoadJSONFile();

            ConnectAgainToDatabase = () => LoadJSONFile();
        }

        #region INoSQLRepository

        public override Task Close()
        {
            SaveJSONFile();
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

            T elt;

            if (!localDb.TryGetValue(id, out elt))
            {
                throw new KeyNotFoundNoSQLException(string.Format("Id '{0}' not found in the repository '{1}'", id, DbFilePath));
            }

            if (elt.Deleted)
            {
                throw new KeyNotFoundNoSQLException(string.Format("Id '{0}' not found in the repository '{1}'", id, DbFilePath));
            }

            if (config.IsExpired(id))
            {
                throw new KeyNotFoundNoSQLException(string.Format("Id '{0}' not found in the repository '{1}'", id, DbFilePath));
            }

            return elt;
        }

        public override IEnumerable<T> GetByIds(IList<string> ids)
        {
            CheckOpenedConnection();

            var elts = new List<T>();

            foreach (string id in ids)
            {
                var elt = TryGetById(id);
                if (elt != null)
                    elts.Add(elt);
            }

            return elts;
        }

        public override InsertResult InsertOne(T entity, InsertMode insertMode)
        {
            CheckOpenedConnection();

            var entitydomain = entity;

            NoSQLRepoHelper.SetIds(entitydomain);

            var updateddate = NoSQLRepoHelper.DateTimeUtcNow();
            var createdDate = NoSQLRepoHelper.DateTimeUtcNow();

            if (!string.IsNullOrEmpty(entity.Id) && localDb.ContainsKey(entity.Id))
            {
                // Document already exists
                switch (insertMode)
                {
                    case InsertMode.error_if_key_exists:
                        throw new DupplicateKeyNoSQLException();
                    case InsertMode.erase_existing:
                        createdDate = localDb[entity.Id].SystemCreationDate;
                        localDb.Remove(entity.Id);
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

            if (localDb.ContainsKey(entity.Id))
                localDb[entity.Id] = entityToStore;
            else
                localDb.Add(entity.Id, entityToStore);

            config.ExpireAt(entity.Id, null);

            SaveJSONFile();

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

            return localDb.ContainsKey(id);
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

            if (!localDb.ContainsKey(entity.Id))
            {
                if (updateMode == UpdateMode.error_if_missing_key)
                    throw new KeyNotFoundNoSQLException("Misssing key '" + entity.Id + "'");
                else if (updateMode == UpdateMode.do_nothing_if_missing_key)
                    return UpdateResult.not_affected;
            }

            localDb[entity.Id] = entityToStore;

            config.ExpireAt(entity.Id, null);

            SaveJSONFile();

            return UpdateResult.updated;
        }

        public override long Delete(string id, bool physical)
        {
            CheckOpenedConnection();

            if (localDb.ContainsKey(id))
            {
                foreach (var attachmentName in GetAttachmentNames(id))
                {
                    RemoveAttachment(id, attachmentName);
                }

                if (physical)
                {
                    localDb.Remove(id);
                    config.Delete(id);
                }
                else
                {
                    localDb[id].Deleted = true;
                }
                SaveJSONFile();
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
                return localDb.Keys.Count(e => !config.IsExpired(e));
            }

            return 0;
        }

        public override void InitCollection()
        {
            CheckOpenedConnection();

            // Autoinit, nothing to do
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

            config.ExpireAt(id, dateLimit);
            SavedDbConfig();
        }

        public override bool CompactDatabase()
        {
            CheckOpenedConnection();

            if (this.localDb != null)
            {
                foreach (var item in localDb.Values.Where(e => e.Deleted || config.IsExpired(e.Id)))
                {
                    Delete(item.Id, true);
                }
            }
            return true;
        }

        public override long TruncateCollection()
        {
            CheckOpenedConnection();

            var count = localDb.Keys.Count;
            localDb = new ConcurrentDictionary<string, T>();
            attachmentDetails = new ConcurrentDictionary<string, AttachmentDetail>();
            config.TruncateCollection();
            SaveJSONFile();
            return count;
        }

        public override void SetCollectionName(string typeName)
        {
            CheckOpenedConnection();

            CollectionName = typeName;
            LoadJSONFile();
        }

        public override void UseDatabase(string dbName)
        {
            CheckOpenedConnection();

            throw new NotImplementedException();
        }

        public override bool CollectionExists(bool createIfNotExists)
        {
            CheckOpenedConnection();

            throw new NotImplementedException();
        }

        public override void DropCollection()
        {
            CheckOpenedConnection();

            this.localDb = new ConcurrentDictionary<string, T>();
            config.TruncateCollection();
            SaveJSONFile();
        }

        #endregion

        #region Private methods

        private void LoadJSONFile()
        {
            if (File.Exists(DbFilePath))
            {
                try
                {
                    var content = File.ReadAllText(DbFilePath);
                    var settings = new JsonSerializerSettings()
                    {
                        TypeNameHandling = TypeNameHandling.Objects,
                        DefaultValueHandling = DefaultValueHandling.Populate
                    };

                    this.localDb = JsonConvert.DeserializeObject<ConcurrentDictionary<string, T>>(content, settings);

                    if (this.localDb == null)
                        this.localDb = new ConcurrentDictionary<string, T>(); // Empty file

                    this.attachmentDetails = new ConcurrentDictionary<string, AttachmentDetail>(); // Empty file
                    if (File.Exists(AttachmentsFilePath))
                    {
                        content = File.ReadAllText(AttachmentsFilePath);
                        this.attachmentDetails = JsonConvert.DeserializeObject<ConcurrentDictionary<string, AttachmentDetail>>(content, settings);
                    }
                }
                catch
                {
                    throw new IOException(string.Format("Cannot read json repository file '{0}'", DbFilePath));
                }
            }
            else
            {
                this.localDb = new ConcurrentDictionary<string, T>();
                this.attachmentDetails = new ConcurrentDictionary<string, AttachmentDetail>();
            }

            LoadDbConfigFile();

            ConnectionOpened = true;
        }

        private void SaveJSONFile()
        {
            var settings = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Auto,
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Ignore,
                // TypeNameAssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Simple
            };

            string content = JsonConvert.SerializeObject(this.localDb, Formatting.Indented, settings);
            if (!Directory.Exists(dbDirectoryPath))
                Directory.CreateDirectory(dbDirectoryPath);

            File.WriteAllText(DbFilePath, content);
            
            content = JsonConvert.SerializeObject(this.attachmentDetails, Formatting.Indented, settings);

            File.WriteAllText(AttachmentsFilePath, content);

            SavedDbConfig();
        }

        private void LoadDbConfigFile()
        {
            if (File.Exists(DbConfigFilePath))
            {
                try
                {
                    var content = File.ReadAllText(DbConfigFilePath);
                    var settings = new JsonSerializerSettings()
                    {
                        TypeNameHandling = TypeNameHandling.Objects,
                        DefaultValueHandling = DefaultValueHandling.Populate
                    };

                    this.config = JsonConvert.DeserializeObject<DbConfiguration>(content, settings);

                    if (this.config == null)
                        this.config = new DbConfiguration(); // Empty file
                }
                catch
                {
                    throw new IOException(string.Format("Cannot read config repository file '{0}'", DbFilePath));
                }
            }
            else
            {
                this.config = new DbConfiguration();
            }
        }

        private void SavedDbConfig()
        {
            var settings = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Auto,
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Ignore
            };

            string content = JsonConvert.SerializeObject(this.config, Formatting.Indented, settings);
            if (!Directory.Exists(dbDirectoryPath))
                Directory.CreateDirectory(dbDirectoryPath);

            File.WriteAllText(DbConfigFilePath, content);
        }

        #endregion

        #region Attachments

        public override void AddAttachment(string id, Stream fileStream, string contentType, string attachmentName)
        {
            CheckOpenedConnection();

            var entityAttachmentDir = Path.Combine(AttachmentsDirectoryPath, id);
            var attachmentFilePath = Path.Combine(entityAttachmentDir, attachmentName);

            if (!Directory.Exists(entityAttachmentDir))
                Directory.CreateDirectory(entityAttachmentDir);

            using (var file = File.OpenWrite(attachmentFilePath))
            {
                fileStream.CopyTo(file);
            }

            attachmentDetails.Add((id + "-" + attachmentName).ToLower(),
                new AttachmentDetail() {
                DocumentId = id,
                ContentType = contentType,
                FileName = attachmentName
            });
        }

        public override void RemoveAttachment(string id, string attachmentName)
        {
            CheckOpenedConnection();

            var entityAttachmentDir = AttachmentsDirectoryPath + "/" + id;
            var attachmentFilePath = entityAttachmentDir + "/" + attachmentName;

            if (!Exist(id))
                throw new KeyNotFoundNoSQLException();

            if (!File.Exists(attachmentFilePath))
                throw new AttachmentNotFoundNoSQLException();

            File.Delete(attachmentFilePath);

            var entryKey = (id + "-" + attachmentName).ToLower();
            if (attachmentDetails.ContainsKey(entryKey))
                attachmentDetails.Remove(entryKey);
        }

        public override Stream GetAttachment(string id, string attachmentName)
        {
            CheckOpenedConnection();

            var entityAttachmentDir = AttachmentsDirectoryPath + "/" + id;
            var attachmentFilePath = entityAttachmentDir + "/" + attachmentName;

            if (!Exist(id))
                throw new KeyNotFoundNoSQLException();

            if (!File.Exists(attachmentFilePath))
                throw new AttachmentNotFoundNoSQLException();

            return File.OpenRead(attachmentFilePath);
        }

        public override AttachmentDetail GetAttachmentDetail(string id, string attachmentName)
        {
            CheckOpenedConnection();

            var entityAttachmentDir = AttachmentsDirectoryPath + "/" + id;
            var attachmentFilePath = entityAttachmentDir + "/" + attachmentName;

            if (!Exist(id))
                throw new KeyNotFoundNoSQLException();

            if (!File.Exists(attachmentFilePath))
                throw new AttachmentNotFoundNoSQLException();

            var entryKey = (id + "-" + attachmentName).ToLower();
            if (attachmentDetails.ContainsKey(entryKey))
                return attachmentDetails[entryKey];
            else
                throw new AttachmentNotFoundNoSQLException();
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
                return localDb.Values.Where(e => !config.IsExpired(e.Id));
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

            var entityAttachmentDir = Path.Combine(AttachmentsDirectoryPath, id);

            if (Directory.Exists(entityAttachmentDir))
            {
                var fullFilePath = Directory.GetFiles(entityAttachmentDir);
                return fullFilePath.Select(file => file.Substring(file.LastIndexOf("\\", StringComparison.Ordinal) + 1));
            }
            return new List<string>();
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

        /// <inheritdoc/>
        public override IEnumerable<T> DoQuery(NoSqlQuery<T> queryFilters)
        {
            var query = localDb.Values.Select(e => e);

            // Filters :
            if (queryFilters.Filter != null)
            {
                var filterFunction = queryFilters.Filter.Compile();
                query = query.Where(e => filterFunction.Invoke(e));
            }

            query = query.OrderBy(e => e.SystemCreationDate);

            if (queryFilters.Skip > 0)
                query = query.Skip(queryFilters.Skip);
            if (queryFilters.Limit > 0)
                query = query.Take(queryFilters.Limit);

            return query;
        }

        /// <inheritdoc/>
        public override IEnumerable<string> GetIds()
        {
            return localDb.Values.AsQueryable()
                                .Where(e => !e.Deleted)
                                .Select(e => e.Id);
        }

        /// <inheritdoc/>
        public override INoSqlQueryable<T> Query()
        {
            return new JsonNoSqlQueryable<T>(this);
        }

        #endregion
    }
}