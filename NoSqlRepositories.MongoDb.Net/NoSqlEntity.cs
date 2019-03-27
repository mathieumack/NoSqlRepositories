using NoSqlRepositories.Core;
using NoSqlRepositories.Core.interfaces;
using System;

namespace NoSqlRepositories.MongoDb
{
    public class NoSqlEntity<T> : INoSqlEntity<T> where T : class, IBaseEntity
    {
        #region Document

        internal T document;

        #endregion

        public string Id
        {
            get
            {
                return document.Id;
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
        /// Creation date of the object in the repository
        /// </summary>
        public DateTimeOffset SystemCreationDate
        {
            get
            {
                return document.SystemCreationDate;
            }
            set
            {
                document.SystemCreationDate = value;
            }
        }

        /// <summary>
        /// Last update date of the object in the repository
        /// </summary>
        public DateTimeOffset SystemLastUpdateDate
        {
            get
            {
                return document.SystemLastUpdateDate;
            }
            set
            {
                document.SystemLastUpdateDate = value;
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
            this.document = document;
        }

        #region Domain entity mapping

        public T GetEntityDomain()
        {
            return document;
        }

        public void SetEntityDomain(T entityModel)
        {
            this.document = entityModel;
        }

        #endregion
    }
}
