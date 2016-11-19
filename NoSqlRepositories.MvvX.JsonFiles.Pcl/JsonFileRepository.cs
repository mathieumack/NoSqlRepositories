﻿using MvvmCross.Plugins.File;
using Newtonsoft.Json;
using NoSqlRepositories.Core;
using NoSqlRepositories.Core.Helpers;
using NoSqlRepositories.Core.NoSQLException;
using NoSqlRepositories.MvvX.JsonFiles.Pcl.Helpers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;

namespace NoSqlRepositories.MvvX.JsonFiles.Pcl
{
    public class JsonFileRepository<T> : RepositoryBase<T> where T : class, IBaseEntity
    {
        #region Members

        public override NoSQLEngineType EngineType
        {
            get
            {
                return NoSQLEngineType.JsonFile;
            }
        }

        private readonly IMvxFileStore fileStore;

        private string dBName { get; set; }

        private string AttachmentsDirectoryPath
        {
            get
            {
                return dBName + "/Attachments/" + CollectionName;
            }
        }

        private string DbDirectoryPath
        {
            get
            {
                return dBName;
            }
        }

        public string DbFilePath
        {
            get
            {
                return dBName + "/" + CollectionName + ".json";
            }
        }

        private IDictionary<string, T> localDb;

        #endregion

        public JsonFileRepository(IMvxFileStore fileStore, string dbName)
        {
            if (fileStore == null)
                throw new ArgumentNullException("fileStore");

            this.fileStore = fileStore;
            this.dBName = dbName;

            CollectionName = typeof(T).Name;
            LoadJSONFile();
        }

        #region INoSQLRepository

        public override T GetById(string id)
        {
            T elt;

            if (!localDb.TryGetValue(id, out elt))
            {
                throw new KeyNotFoundNoSQLException(string.Format("Id '{0}' not found in the repository '{1}'", id, DbFilePath));
            }

            if (elt.Deleted)
            {
                throw new KeyNotFoundNoSQLException(string.Format("Id '{0}' not found in the repository '{1}'", id, DbFilePath));
            }

            return elt;
        }

        public override InsertResult InsertOne(T entity, InsertMode keyExistsAction)
        {
            var insertResult = default(InsertResult);

            var date = NoSQLRepoHelper.DateTimeUtcNow();
            if (AutoGeneratedEntityDate)
            {
                entity.SystemCreationDate = date;
                entity.SystemLastUpdateDate = date;
            }

            NoSQLRepoHelper.SetIds(entity);

            if (localDb.ContainsKey(entity.Id))
            {
                if (keyExistsAction == InsertMode.error_if_key_exists)
                {
                    throw new DupplicateKeyNoSQLException();
                }
                else if (keyExistsAction == InsertMode.do_nothing_if_key_exists)
                {
                    return InsertResult.not_affected;
                }
                else if (keyExistsAction == InsertMode.erase_existing)
                {
                    entity.SystemCreationDate = localDb[entity.Id].SystemCreationDate; // keep the origin creation date of the entity
                    // Continue execution
                }
                insertResult = InsertResult.updated;
            }
            else
            {
                insertResult = InsertResult.inserted;
            }

            // Clone to not shared reference between App instance and repository persisted value
            // UTC : Ensure to store only utc datetime
            var entityToStore = NewtonJsonHelper.CloneJson(entity, DateTimeZoneHandling.Utc);

            localDb[entity.Id] = entityToStore;

            SaveJSONFile();
            return insertResult;
        }

        public override BulkInsertResult<string> InsertMany(IEnumerable<T> entities, InsertMode insertMode)
        {
            if (insertMode != InsertMode.db_implementation)
                throw new NotImplementedException();

            var insertResult = new BulkInsertResult<string>();

            var creationDate = NoSQLRepoHelper.DateTimeUtcNow();

            foreach (var entity in entities)
            {
                if (AutoGeneratedEntityDate)
                    entity.SystemCreationDate = creationDate;
                if (AutoGeneratedEntityDate)
                    entity.SystemLastUpdateDate = creationDate;

                NoSQLRepoHelper.SetIds(entity);

                var entityToStore = NewtonJsonHelper.CloneJson(entity, DateTimeZoneHandling.Utc);
                localDb[entity.Id] = entityToStore;
                insertResult[entity.Id] = InsertResult.unknown;
            }
            SaveJSONFile();

            return insertResult;
        }

        public override bool Exist(string id)
        {
            return localDb.Keys.Contains(id);
        }

        public override UpdateResult Update(T entity, UpdateMode updateMode)
        {
            var updateResult = default(UpdateResult);

            if (updateMode == UpdateMode.upsert_if_missing_key)
            {
                throw new NotImplementedException();
            }

            var updateDate = NoSQLRepoHelper.DateTimeUtcNow();

            entity.SystemLastUpdateDate = updateDate;

            if (updateMode == UpdateMode.upsert_if_missing_key)
            {
                NoSQLRepoHelper.SetIds(entity);
                entity.SystemCreationDate = updateDate;
            }

            if (!localDb.ContainsKey(entity.Id))
            {
                if (updateMode == UpdateMode.error_if_missing_key)
                {
                    throw new KeyNotFoundNoSQLException("Misssing key '" + entity.Id + "'");
                }
                else if (updateMode == UpdateMode.do_nothing_if_missing_key)
                {
                    return UpdateResult.not_affected;
                }

                updateResult = UpdateResult.inserted;
            }

            localDb[entity.Id] = entity;

            SaveJSONFile();
            updateResult = UpdateResult.updated;

            return updateResult;
        }

        public override long Delete(string id, bool physical)
        {
            if (!physical)
                throw new NotImplementedException();

            if (localDb.ContainsKey(id))
            {
                foreach (var attachmentName in GetAttachmentNames(id))
                {
                    RemoveAttachment(id, attachmentName);
                }

                if (physical)
                {
                    localDb.Remove(id);
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
            T res;

            try
            {
                res = GetById(id);
            }
            catch (KeyNotFoundNoSQLException)
            {
                res = default(T);
            }
            return res;
        }

        public override void InitCollection(List<Expression<Func<T, object>>> indexFieldSelectors)
        {
            // Nothing to do to initialize the collection
        }

        #endregion

        #region INoSQLDB

        public override long TruncateCollection()
        {
            var count = localDb.Keys.Count;
            localDb = new ConcurrentDictionary<string, T>();
            SaveJSONFile();
            return count;
        }

        public override void SetCollectionName(string typeName)
        {
            CollectionName = typeName;
            LoadJSONFile();
        }

        public override void InitCollection()
        {
            // Autoinit, nothing to do
        }

        public override void UseDatabase(string dbName)
        {
            throw new NotImplementedException();
        }

        public override bool CollectionExists(bool createIfNotExists)
        {
            throw new NotImplementedException();
        }

        public override void DropCollection()
        {
            this.localDb = new Dictionary<string, T>();
            SaveJSONFile();
        }

        #endregion

        #region Private methods

        private void LoadJSONFile()
        {
            //if (File.Exists(DbFilePath))
            if (fileStore.Exists(DbFilePath))
            {

                string content = null;
                if (fileStore.TryReadTextFile(DbFilePath, out content))
                {
                    var settings = new JsonSerializerSettings()
                    {
                        TypeNameHandling = TypeNameHandling.Objects,
                        DefaultValueHandling = DefaultValueHandling.Populate
                    };

                    this.localDb = JsonConvert.DeserializeObject<IDictionary<string, T>>(content, settings);

                    if (this.localDb == null)
                        this.localDb = new ConcurrentDictionary<string, T>(); // Empty file
                }
                else
                {
                    throw new IOException(string.Format("Cannot read json repository file '{0}'", DbFilePath));
                }
            }
            else
            {
                this.localDb = new Dictionary<string, T>();
            }
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
            fileStore.EnsureFolderExists(DbDirectoryPath);
            fileStore.WriteFile(DbFilePath, content);
        }

        #endregion

        #region Attachments

        public override void AddAttachment(string id, Stream fileStream, string contentType, string attachmentName)
        {
            var entityAttachmentDir = fileStore.PathCombine(AttachmentsDirectoryPath, id);
            var attachmentFilePath = fileStore.PathCombine(entityAttachmentDir, attachmentName);
            fileStore.EnsureFolderExists(entityAttachmentDir);

            fileStore.WriteFile(attachmentFilePath, (localFileStream) => fileStream.CopyTo(localFileStream));
        }

        public override void RemoveAttachment(string id, string attachmentName)
        {
            var entityAttachmentDir = AttachmentsDirectoryPath + "/" + id;
            var attachmentFilePath = entityAttachmentDir + "/" + attachmentName;

            if (!Exist(id))
                throw new KeyNotFoundNoSQLException();

            if (!fileStore.Exists(attachmentFilePath))
                throw new AttachmentNotFoundNoSQLException();

            fileStore.DeleteFile(attachmentFilePath);
        }

        public override Stream GetAttachment(string id, string attachmentName)
        {
            var entityAttachmentDir = AttachmentsDirectoryPath + "/" + id;
            var attachmentFilePath = entityAttachmentDir + "/" + attachmentName;

            if (!Exist(id))
                throw new KeyNotFoundNoSQLException();

            if (!fileStore.Exists(attachmentFilePath))
                throw new AttachmentNotFoundNoSQLException();

            return fileStore.OpenRead(attachmentFilePath);
        }

        /// <summary>
        /// Return all entities of repository
        /// </summary>
        /// <returns></returns>
        public override IList<T> GetAll()
        {
            List<T> result = new List<T>();
            LoadJSONFile();
            if (this.localDb != null)
            {
                foreach (var key in localDb.Keys)
                {
                    result.Add(localDb[key]);
                }
            }

            return result;
        }

        /// <summary>
        /// Return the list of name of all attachements of a given entity
        /// </summary>
        /// <param name="idDocument"></param>
        /// <returns></returns>
        public override IList<string> GetAttachmentNames(string idDocument)
        {
            var result = new List<string>();
            var entityAttachmentDir = fileStore.PathCombine(AttachmentsDirectoryPath, idDocument);

            if (fileStore.FolderExists(entityAttachmentDir))
            {
                var fullFilePath = fileStore.GetFilesIn(entityAttachmentDir);
                result = fullFilePath.Select(file => file.Substring(file.LastIndexOf("\\", StringComparison.Ordinal) + 1)).ToList();
            }
            return result;
        }

        public override List<T> GetByField<TField>(string fieldName, List<TField> values)
        {
            throw new NotImplementedException();
        }

        public override List<T> GetByField<TField>(string fieldName, TField value)
        {
            throw new NotImplementedException();
        }

        public override List<string> GetKeyByField<TField>(string fieldName, List<TField> values)
        {
            throw new NotImplementedException();
        }

        public override List<string> GetKeyByField<TField>(string fieldName, TField value)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
