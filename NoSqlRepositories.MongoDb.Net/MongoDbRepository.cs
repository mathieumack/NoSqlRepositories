﻿using NoSqlRepositories.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using MongoDB.Driver;
using System.Security.Authentication;
using NoSqlRepositories.Core.NoSQLException;
using MongoDB.Bson;
using NoSqlRepositories.Core.Helpers;
using MongoDB.Driver.GridFS;
using System.Linq;

namespace NoSqlRepositories.MongoDb.Net
{
    public class MongoDbRepository<T> : RepositoryBase<T> where T : class, IBaseEntity
    {
        #region private fields

        protected IMongoClient client;
        protected IMongoDatabase database;
        protected IMongoCollection<T> collection;

        private string databaseName;
        public string TypeName { get; set; }

        public override NoSQLEngineType EngineType
        {
            get
            {
                return NoSQLEngineType.MongoDb;
            }
        }

        public override string DatabaseName
        {
            get
            {
                return databaseName;
            }
        }

        #endregion

        #region Constructor

        public MongoDbRepository(string mongoDbUrl, string databaseName)
            : this(mongoDbUrl, databaseName, typeof(T).Name.ToString())
        {
        }

        public MongoDbRepository(string mongoDbUrl, string databaseName, string collectionName)
        {
            TypeName = typeof(T).Name;
            this.client = new MongoClient();

            MongoClientSettings settings = MongoClientSettings.FromUrl(new MongoUrl(mongoDbUrl));
            settings.SslSettings = new SslSettings() { EnabledSslProtocols = SslProtocols.Tls12 };

            client = new MongoClient(settings);
            database = client.GetDatabase(databaseName);
            CollectionName = collectionName;
            collection = database.GetCollection<T>(collectionName);
        }

        public override T GetById(string id)
        {
            T elt = collection.Find(e => e.Id != default(string) && e.Id.Equals(id)).FirstOrDefault();
            if (elt == default(T) || elt.Deleted)
            {
                throw new KeyNotFoundNoSQLException(string.Format("Id '{0}' not found in the repository '{1}'", id, CollectionName));
            }

            // Does not support yet expiration on MongoDb. Solution to be found
            //if (elt.IsExpired(id))
            //{
            //    throw new KeyNotFoundNoSQLException(string.Format("Id '{0}' not found in the repository '{1}'", id, CollectionName));
            //}

            return elt;
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

        public override InsertResult InsertOne(T entity, InsertMode insertMode)
        {
            if(insertMode == InsertMode.erase_existing && Exist(entity.Id))
            {
                var existingEntity = GetById(entity.Id);

                entity.SystemCreationDate = existingEntity.SystemCreationDate;
                Delete(existingEntity.Id, true);
            }
            try
            {
                try
                {
                    var date = NoSQLRepoHelper.DateTimeUtcNow();
                    if (AutoGeneratedEntityDate)
                    {
                        if(insertMode != InsertMode.erase_existing)
                            entity.SystemCreationDate = date;

                        entity.SystemLastUpdateDate = date;
                    }

                    collection.InsertOne(entity);
                    return InsertResult.inserted;
                }
                catch (AggregateException e)
                {
                    throw e.InnerException;
                }
            }
            catch (MongoWriteException e)
            {
                
                if (e.WriteError.Category == ServerErrorCategory.DuplicateKey && insertMode == InsertMode.error_if_key_exists)
                {
                    throw new DupplicateKeyNoSQLException("The key '" + entity.Id + "' already exists", e);
                }
                if (e.WriteError.Category == ServerErrorCategory.DuplicateKey && insertMode == InsertMode.do_nothing_if_key_exists)
                {
                    return InsertResult.not_affected;
                }
                else if(e.WriteError.Category == ServerErrorCategory.DuplicateKey)
                {
                    return InsertResult.duplicate_key_exception;
                }
                else
                {
                    throw;
                }
            }
        }

        public override BulkInsertResult<string> InsertMany(IEnumerable<T> entities, InsertMode insertMode)
        {
            var insertResult = new BulkInsertResult<string>();

            //TODO : restore Parralel.ForEach
            foreach (var entity in entities)
            {
                // Create the document
                InsertOne(entity, insertMode);
                insertResult[entity.Id] = InsertResult.unknown;
            };
            return insertResult;
        }

        public override bool Exist(string id)
        {
            return collection.Find(e => e.Id != default(string) && e.Id.Equals(id)).Any();
        }

        public override Core.UpdateResult Update(T entity, UpdateMode updateMode)
        {
            Core.UpdateResult updateResult = default(Core.UpdateResult);

            if (updateMode == UpdateMode.db_implementation)
            {
                var updateDate = NoSQLRepoHelper.DateTimeUtcNow();
                entity.SystemLastUpdateDate = updateDate;

                var filter = Builders<T>.Filter.Eq(s => s.Id, entity.Id);

                ReplaceOneResult res = collection.ReplaceOne(filter, entity, new UpdateOptions { IsUpsert = false });

                if (!res.IsAcknowledged)
                {
                    throw new QueryNotAcknowledgedException();
                }

                if (entity.Id == null)
                    throw new ArgumentException("Cannot update an entity with a null field value");

                updateResult = Core.UpdateResult.updated;
            }
            else
            {
                throw new NotImplementedException();
            }

            return updateResult;
        }

        public override long Delete(string id, bool physical)
        {
            try
            {
                long nbElementDeleted = DeleteCore(Builders<T>.Filter.Eq(e => e.Id, id));

                foreach (var attachmentName in GetAttachmentNames(id))
                {
                    RemoveAttachment(id, attachmentName);
                }

                return nbElementDeleted;
            }
            catch (AggregateException e)
            {
                throw e.InnerException;
            }
        }

        private long DeleteCore(FilterDefinition<T> filter)
        {
            DeleteResult res = collection.DeleteMany(filter);

            if (!res.IsAcknowledged)
            {
                throw new QueryNotAcknowledgedException();
            }

            return res.DeletedCount;
        }

        public override void InitCollection()
        {
            //try
            //{
                collection = this.database.GetCollection<T>(TypeName);
            //}
            //catch (DocumentClientException de)
            //{
            //    // If the document collection does not exist, create a new collection
            //    if (de.StatusCode == HttpStatusCode.NotFound)
            //    {
            //        DocumentCollection collectionInfo = new DocumentCollection();

            //        collectionInfo.Id = TypeName;

            //        // Optionally, you can configure the indexing policy of a collection. Here we configure collections for maximum query flexibility 
            //        // including string range queries. 
            //        collectionInfo.IndexingPolicy = new IndexingPolicy(new RangeIndex(DataType.String) { Precision = -1 });

            //        // DocumentDB collections can be reserved with throughput specified in request units/second. 1 RU is a normalized request equivalent to the read
            //        // of a 1KB document.  Here we create a collection with 400 RU/s. 
            //        await this.client.CreateDocumentCollectionAsync(
            //            UriFactory.CreateDatabaseUri(databaseName),
            //            new DocumentCollection { Id = TypeName },
            //            new RequestOptions { OfferThroughput = 400 });
            //    }
            //    else
            //    {
            //        throw;
            //    }
            //}
        }

        public override void InitCollection(IList<Expression<Func<T, object>>> indexFieldSelectors)
        {
            throw new NotImplementedException();
        }

        public override void AddAttachment(string id, Stream fileStream, string contentType, string attachmentName)
        {
            var bucket = new GridFSBucket(database, new GridFSBucketOptions()
            {
                BucketName = "attachments"
            });

            var options = new GridFSUploadOptions
            {
                ChunkSizeBytes = 1048576, // 1MB
                Metadata = new BsonDocument
                {
                    { "content-type", contentType },
                    { "collection", CollectionName },
                    { "document-id", id },
                    { "attachment-name", attachmentName },
                    { "attachment-id", id + "-" + attachmentName },
                }
            };

            bucket.UploadFromStream(attachmentName, fileStream, options);
        }

        public override void RemoveAttachment(string id, string attachmentName)
        {
            var bucket = new GridFSBucket(database, new GridFSBucketOptions()
            {
                BucketName = "attachments"
            });

            var document = bucket.Find(Builders<GridFSFileInfo>.Filter.Eq(e => e.Metadata["attachment-id"], id + "-" + attachmentName)).FirstOrDefault();
            if (document != null)
            {
                bucket.Delete(document.Id);
            }
            else
                throw new AttachmentNotFoundNoSQLException();
        }

        public override Stream GetAttachment(string id, string attachmentName)
        {
            var bucket = new GridFSBucket(database, new GridFSBucketOptions()
            {
                BucketName = "attachments"
            });

            var document = bucket.Find(Builders<GridFSFileInfo>.Filter.Eq(e => e.Metadata["attachment-id"], id + "-" + attachmentName)).FirstOrDefault();
            if(document != null)
            {
                Stream destination = new MemoryStream();
                bucket.DownloadToStream(document.Id, destination);
                destination.Position = 0;
                return destination;
            }
            else
                throw new AttachmentNotFoundNoSQLException();
        }

        public override IEnumerable<string> GetAttachmentNames(string id)
        {
            var bucket = new GridFSBucket(database, new GridFSBucketOptions()
            {
                BucketName = "attachments"
            });

            var document = bucket.Find(Builders<GridFSFileInfo>.Filter.Eq(e => e.Metadata["document-id"], id)).ToList();
            return document.Select(e => e.Metadata["attachment-name"].AsString);
        }

        public override IEnumerable<T> GetAll()
        {
            return collection.Find(e => !e.Deleted).ToEnumerable();
        }

        public override IEnumerable<T> GetByField<TField>(string fieldName, List<TField> values)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<T> GetByField<TField>(string fieldName, TField value)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<string> GetKeyByField<TField>(string fieldName, List<TField> values)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<string> GetKeyByField<TField>(string fieldName, TField value)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Create a database with the specified name if it doesn't exist. 
        /// </summary>
        /// <param name="databaseName">The name/ID of the database.</param>
        /// <returns>The Task for asynchronous execution.</returns>
        public override void UseDatabase(string dbName)
        {
            this.databaseName = dbName;
            this.database = client.GetDatabase(dbName);
        }
        
        public override long TruncateCollection()
        {
            return DeleteCore(Builders<T>.Filter.Empty);
        }

        public override void DropCollection()
        {
            this.database.DropCollection(CollectionName);
        }

        public override void SetCollectionName(string typeName)
        {
            throw new NotImplementedException();
        }

        public override bool CollectionExists(bool createIfNotExists)
        {
            //try
            //{
                collection = this.database.GetCollection<T>(TypeName);
                if(collection == null && createIfNotExists)
                {
                    this.database.CreateCollection(TypeName);
                }
                collection = this.database.GetCollection<T>(TypeName);
                return collection != null;
            //}
            //catch (DocumentClientException de)
            //{
            //    // If the document collection does not exist, create a new collection
            //    if (de.StatusCode == HttpStatusCode.NotFound)
            //    {
            //        DocumentCollection collectionInfo = new DocumentCollection();

            //        collectionInfo.Id = TypeName;

            //        // Optionally, you can configure the indexing policy of a collection. Here we configure collections for maximum query flexibility 
            //        // including string range queries. 
            //        collectionInfo.IndexingPolicy = new IndexingPolicy(new RangeIndex(DataType.String) { Precision = -1 });

            //        // DocumentDB collections can be reserved with throughput specified in request units/second. 1 RU is a normalized request equivalent to the read
            //        // of a 1KB document.  Here we create a collection with 400 RU/s. 
            //        await this.client.CreateDocumentCollectionAsync(
            //            UriFactory.CreateDatabaseUri(databaseName),
            //            new DocumentCollection { Id = TypeName },
            //            new RequestOptions { OfferThroughput = 400 });
            //        return true;
            //    }
            //    else
            //    {
            //        return false;
            //    }
            //}
        }

        public override bool CompactDatabase()
        {
            database.RunCommand<BsonDocument>(new BsonDocument("compact", CollectionName));
            return true;
        }

        public override void ExpireAt(string id, DateTime? dateLimit)
        {
            throw new NotImplementedException();
        }

        public override IList<T> GetByIds(IList<string> ids)
        {
            return collection.Find(e => ids.Contains(e.Id)).ToList();
        }

        public override int Count()
        {
            return (int)collection.EstimatedDocumentCount();
        }

        #endregion
    }
}
