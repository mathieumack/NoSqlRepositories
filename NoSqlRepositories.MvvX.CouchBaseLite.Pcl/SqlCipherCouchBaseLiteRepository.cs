using System;
using MvvX.Plugins.CouchBaseLite;
using MvvX.Plugins.CouchBaseLite.Storages;
using NoSqlRepositories.Core;

namespace NoSqlRepositories.MvvX.CouchBaseLite.Pcl
{
    /// <summary>
    /// The repository can contains instance of subtype of the type T and handle polymorphism. For that
    /// the subclasses of class T must be declared in the attribute PolymorphicTypes
    /// Limitations : CouchBaseLite repository doesn't handle polymorphism in attribute's entity of type List, Dictionary...
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SqlCipherCouchBaseLiteRepository<T> : CouchBaseLiteRepository<T> where T : class, IBaseEntity
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="CouchBaseLiteLite"></param>
        /// <param name="dbName"></param>
        /// <param name="password"></param>
        public SqlCipherCouchBaseLiteRepository(ICouchBaseLite CouchBaseLiteLite, 
                                                string dbName,
                                                string password)
        {
            Construct(CouchBaseLiteLite, StorageTypes.Sqlite, dbName, password);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="CouchBaseLiteLite"></param>
        /// <param name="dbName"></param>
        /// <param name="keyData"></param>
        public SqlCipherCouchBaseLiteRepository(ICouchBaseLite CouchBaseLiteLite,
                                                string dbName,
                                                byte[] keyData)
        {
            Construct(CouchBaseLiteLite, StorageTypes.Sqlite, dbName, keyData);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="CouchBaseLiteLite"></param>
        /// <param name="dbName"></param>
        /// <param name="password"></param>
        /// <param name="salt"></param>
        /// <param name="rounds"></param>
        public SqlCipherCouchBaseLiteRepository(ICouchBaseLite CouchBaseLiteLite,
                                                string dbName,
                                                string password,
                                                byte[] salt,
                                                int rounds)
        {
            Construct(CouchBaseLiteLite, StorageTypes.Sqlite, dbName, password, salt, rounds);
        }

        private void Construct(ICouchBaseLite CouchBaseLiteLite, 
                                StorageTypes storage, 
                                string dbName,
                                string password)
        {
            if (CouchBaseLiteLite == null)
                throw new ArgumentNullException("CouchBaseLiteLite");

            this.CouchBaseLiteLite = CouchBaseLiteLite;
            this.CollectionName = typeof(T).Name;

            ConnectToDatabase(storage, dbName, password);

            CreateAllDocView();
        }

        private void Construct(ICouchBaseLite CouchBaseLiteLite,
                                StorageTypes storage,
                                string dbName,
                                byte[] keyData)
        {
            if (CouchBaseLiteLite == null)
                throw new ArgumentNullException("CouchBaseLiteLite");

            this.CouchBaseLiteLite = CouchBaseLiteLite;
            this.CollectionName = typeof(T).Name;

            ConnectToDatabase(storage, dbName, keyData);

            CreateAllDocView();
        }

        private void Construct(ICouchBaseLite CouchBaseLiteLite,
                                StorageTypes storage,
                                string dbName,
                                string password,
                                byte[] salt,
                                int rounds)
        {
            if (CouchBaseLiteLite == null)
                throw new ArgumentNullException("CouchBaseLiteLite");

            this.CouchBaseLiteLite = CouchBaseLiteLite;
            this.CollectionName = typeof(T).Name;

            ConnectToDatabase(storage, dbName, password, salt, rounds);

            CreateAllDocView();
        }

        private void ConnectToDatabase(StorageTypes storage, string dbName, string password)
        {
            var databaseOptions = this.CouchBaseLiteLite.CreateDatabaseOptions();
            databaseOptions.Create = true;
            databaseOptions.SetSymmetricKey(password);
            databaseOptions.StorageType = storage;

            this.database = this.CouchBaseLiteLite.CreateConnection(dbName, databaseOptions);

            if (this.database == null)
                throw new NullReferenceException("CreateConnection returned no connection");
        }

        private void ConnectToDatabase(StorageTypes storage, string dbName, byte[] keyData)
        {
            var databaseOptions = this.CouchBaseLiteLite.CreateDatabaseOptions();
            databaseOptions.Create = true;
            databaseOptions.SetSymmetricKey(keyData);
            databaseOptions.StorageType = storage;

            this.database = this.CouchBaseLiteLite.CreateConnection(dbName, databaseOptions);

            if (this.database == null)
                throw new NullReferenceException("CreateConnection returned no connection");
        }

        private void ConnectToDatabase(StorageTypes storage, string dbName, string password, byte[] salt, int rounds)
        {
            var databaseOptions = this.CouchBaseLiteLite.CreateDatabaseOptions();
            databaseOptions.Create = true;
            databaseOptions.SetSymmetricKey(password, salt, rounds);
            databaseOptions.StorageType = storage;

            this.database = this.CouchBaseLiteLite.CreateConnection(dbName, databaseOptions);

            if (this.database == null)
                throw new NullReferenceException("CreateConnection returned no connection");
        }
    }
}
