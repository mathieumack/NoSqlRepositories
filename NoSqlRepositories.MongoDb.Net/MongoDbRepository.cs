using NoSqlRepositories.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq.Expressions;
using System.Net;
using System.Threading.Tasks;
using MongoDB.Driver;

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
                return NoSQLEngineType.AzureDocumentDb;
            }
        }

        #endregion

        #region Constructor

        public MongoDbRepository()
        {
            TypeName = typeof(T).Name;
            this.client = new MongoClient();
        }

        public override T GetById(string id)
        {
            throw new NotImplementedException();
        }

        public override T TryGetById(string id)
        {
            throw new NotImplementedException();
        }

        public override InsertResult InsertOne(T entity, InsertMode insertMode)
        {
            throw new NotImplementedException();
        }

        public override BulkInsertResult<string> InsertMany(IEnumerable<T> entities, InsertMode insertMode)
        {
            throw new NotImplementedException();
        }

        public override bool Exist(string id)
        {
            throw new NotImplementedException();
        }

        public override Core.UpdateResult Update(T entity, UpdateMode updateMode)
        {
            throw new NotImplementedException();
        }

        public override long Delete(string id, bool physical)
        {
            try
            {
                var resultDelete = await this.database.DeleteDocumentAsync(UriFactory.CreateDocumentUri(databaseName, CollectionName, id));
                Debug.WriteLine("Deleted document {0}", id);
                return 1;
            }
            catch (DocumentClientException)
            {
                throw;
            }
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

        public override void InitCollection(List<Expression<Func<T, object>>> indexFieldSelectors)
        {
            throw new NotImplementedException();
        }

        public override void AddAttachment(string id, Stream fileStream, string contentType, string attachmentName)
        {
            throw new NotImplementedException();
        }

        public override void RemoveAttachment(string id, string attachmentName)
        {
            throw new NotImplementedException();
        }

        public override Stream GetAttachment(string id, string attachmentName)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<string> GetAttachmentNames(string id)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        public override void DropCollection()
        {
            this.database.DropCollection(TypeName);
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

        #endregion
    }
}
