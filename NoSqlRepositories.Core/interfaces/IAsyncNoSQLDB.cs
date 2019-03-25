//using NoSqlRepositories.Core;
//using System.Threading.Tasks;

//namespace NoSqlRepositories.Core
//{
//    public interface IAsyncNoSQLDB<T> where T : IBaseEntity
//    {
//        /// <summary>
//        /// Type of the current engine
//        /// </summary>
//        NoSQLEngineType EngineType { get; }

//        /// <summary>
//        /// Change the active database
//        /// </summary>
//        /// <param name="dbName"></param>
//        Task UseDatabase(string dbName);

//        Task<long> TruncateCollection();

//        Task DropCollection();

//        Task SetCollectionName(string typeName);

//        Task<string> GetCollectionName();

//        /// <summary>
//        /// Run initilization command of the collection (if required by the repository implementation)
//        /// </summary>
//        /// <returns>True if the collection has been initialized</returns>
//        Task InitCollection();

//        /// <summary>
//        /// Check if the collection exists.
//        /// </summary>
//        /// <param name="createIfNotExists">If true, the collection will be created if it not already exists</param>
//        /// <returns>True if the collection exists, else false</returns>
//        Task<bool> CollectionExists(bool createIfNotExists);
//    }
//}