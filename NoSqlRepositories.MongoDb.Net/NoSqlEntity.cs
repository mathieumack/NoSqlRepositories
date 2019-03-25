using NoSqlRepositories.Core;
using NoSqlRepositories.Core.interfaces;

namespace NoSqlRepositories.MongoDb
{
    public class NoSqlEntity<T> : INoSqlEntity<T> where T : class, IBaseEntity
    {
        #region Document
        
        internal T Document { get; private set; }

        #endregion

        public string Id {
            get
            {
                return Document.Id;
            }
        }

        public T DomainEntity
        {
            get
            {
                return Document;
            }
            set
            {
                Document = value;
            }
        }

        private readonly string collectionName;
        public string CollectionName
        {
            get
            {
                return collectionName;
            }
        }

        /// <summary>
        /// Allow you to create a new document
        /// </summary>
        /// <param name="collectionName"></param>
        /// <param name="document"></param>
        public NoSqlEntity(string collectionName, T document)
        {
            this.collectionName = collectionName;
            Document = document;
        }
    }
}
