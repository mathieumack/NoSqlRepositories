using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using NoSqlRepositories.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq.Expressions;
using System.Net;
using System.Threading.Tasks;

namespace NoSqlRepositories.AzureDocumentDb.Net
{
    public class AsyncAzureDocumentDbRepository<T> : AsyncRepositoryBase<T> where T : class, IBaseEntity
    {
        #region private fields

        private readonly DocumentClient client;
        private string databaseName;
        public string TypeName { get; set; }

        public override NoSQLEngineType EngineType
        {
            get
            {
                return NoSQLEngineType.AzureDb;
            }
        }

        #endregion

        #region Constructor

        public AsyncAzureDocumentDbRepository(string endPointUri, string primaryKey)
        {
            if (string.IsNullOrWhiteSpace(endPointUri))
                throw new ArgumentNullException();
            if (string.IsNullOrWhiteSpace(primaryKey))
                throw new ArgumentNullException();

            TypeName = typeof(T).Name;
            this.client = new DocumentClient(new Uri(endPointUri), primaryKey);
        }

        public override async Task<T> GetById(string id)
        {
            throw new NotImplementedException();
        }

        public override async Task<T> TryGetById(string id)
        {
            throw new NotImplementedException();
        }

        public override async Task<InsertResult> InsertOne(T entity, InsertMode insertMode)
        {
            throw new NotImplementedException();
        }

        public override async Task<BulkInsertResult<string>> InsertMany(IEnumerable<T> entities, InsertMode insertMode)
        {
            throw new NotImplementedException();
        }

        public override async Task<bool> Exist(string id)
        {
            throw new NotImplementedException();
        }

        public override async Task<UpdateResult> Update(T entity, UpdateMode updateMode)
        {
            throw new NotImplementedException();
        }

        public override async Task<long> Delete(string id, bool physical)
        {
            try
            {
                var resultDelete = await this.client.DeleteDocumentAsync(UriFactory.CreateDocumentUri(databaseName, CollectionName, id));
                Debug.WriteLine("Deleted document {0}", id);
                return 1;
            }
            catch (DocumentClientException de)
            {
                throw;
            }
        }

        public override async Task InitCollection()
        {
            try
            {
                await this.client.ReadDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri(databaseName, TypeName));
            }
            catch (DocumentClientException de)
            {
                // If the document collection does not exist, create a new collection
                if (de.StatusCode == HttpStatusCode.NotFound)
                {
                    DocumentCollection collectionInfo = new DocumentCollection();

                    collectionInfo.Id = TypeName;

                    // Optionally, you can configure the indexing policy of a collection. Here we configure collections for maximum query flexibility 
                    // including string range queries. 
                    collectionInfo.IndexingPolicy = new IndexingPolicy(new RangeIndex(DataType.String) { Precision = -1 });

                    // DocumentDB collections can be reserved with throughput specified in request units/second. 1 RU is a normalized request equivalent to the read
                    // of a 1KB document.  Here we create a collection with 400 RU/s. 
                    await this.client.CreateDocumentCollectionAsync(
                        UriFactory.CreateDatabaseUri(databaseName),
                        new DocumentCollection { Id = TypeName },
                        new RequestOptions { OfferThroughput = 400 });
                }
                else
                {
                    throw;
                }
            }
        }

        public override async Task InitCollection(List<Expression<Func<T, object>>> indexFieldSelectors)
        {
            throw new NotImplementedException();
        }

        public override async Task AddAttachment(string id, Stream fileStream, string contentType, string attachmentName)
        {
            throw new NotImplementedException();
        }

        public override async Task RemoveAttachment(string id, string attachmentName)
        {
            throw new NotImplementedException();
        }

        public override async Task<Stream> GetAttachment(string id, string attachmentName)
        {
            throw new NotImplementedException();
        }

        public override async Task<IList<string>> GetAttachmentNames(string id)
        {
            throw new NotImplementedException();
        }

        public override async Task<IList<T>> GetAll()
        {
            throw new NotImplementedException();
        }

        public override async Task<List<T>> GetByField<TField>(string fieldName, List<TField> values)
        {
            throw new NotImplementedException();
        }

        public override async Task<List<T>> GetByField<TField>(string fieldName, TField value)
        {
            throw new NotImplementedException();
        }

        public override async Task<List<string>> GetKeyByField<TField>(string fieldName, List<TField> values)
        {
            throw new NotImplementedException();
        }

        public override async Task<List<string>> GetKeyByField<TField>(string fieldName, TField value)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Create a database with the specified name if it doesn't exist. 
        /// </summary>
        /// <param name="databaseName">The name/ID of the database.</param>
        /// <returns>The Task for asynchronous execution.</returns>
        public override async Task UseDatabase(string dbName)
        {
            this.databaseName = dbName;
            // Check to verify a database with the id=FamilyDB_vg does not exist
            try
            {
                await this.client.ReadDatabaseAsync(UriFactory.CreateDatabaseUri(databaseName));
            }
            catch (DocumentClientException de)
            {
                // If the database does not exist, create a new database
                if (de.StatusCode == HttpStatusCode.NotFound)
                {
                    await this.client.CreateDatabaseAsync(new Database { Id = databaseName });
                }
                else
                {
                    throw;
                }
            }
        }
        
        public override async Task<long> TruncateCollection()
        {
            throw new NotImplementedException();
        }

        public override async Task DropCollection()
        {
            throw new NotImplementedException();
        }

        public override async Task SetCollectionName(string typeName)
        {
            throw new NotImplementedException();
        }

        public override async Task<bool> CollectionExists(bool createIfNotExists)
        {
            try
            {
                await this.client.ReadDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri(databaseName, TypeName));
                return true;
            }
            catch (DocumentClientException de)
            {
                // If the document collection does not exist, create a new collection
                if (de.StatusCode == HttpStatusCode.NotFound)
                {
                    DocumentCollection collectionInfo = new DocumentCollection();

                    collectionInfo.Id = TypeName;

                    // Optionally, you can configure the indexing policy of a collection. Here we configure collections for maximum query flexibility 
                    // including string range queries. 
                    collectionInfo.IndexingPolicy = new IndexingPolicy(new RangeIndex(DataType.String) { Precision = -1 });

                    // DocumentDB collections can be reserved with throughput specified in request units/second. 1 RU is a normalized request equivalent to the read
                    // of a 1KB document.  Here we create a collection with 400 RU/s. 
                    await this.client.CreateDocumentCollectionAsync(
                        UriFactory.CreateDatabaseUri(databaseName),
                        new DocumentCollection { Id = TypeName },
                        new RequestOptions { OfferThroughput = 400 });
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        #endregion

    }
}
