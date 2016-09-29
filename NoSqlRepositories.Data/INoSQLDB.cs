using NoSqlRepositories.Core;

namespace NoSqlRepositories.Data
{
    public interface INoSQLDB<T> where T : IBaseEntity
    {
        /// <summary>
        /// Change the active database
        /// </summary>
        /// <param name="dbName"></param>
        void UseDatabase(string dbName);

        long TruncateCollection();

        void DropCollection();

        void SetCollectionName(string typeName);

        string GetCollectionName();

        /// <summary>
        /// Run initilization command of the collection (if required by the repository implementation)
        /// </summary>
        /// <returns>True if the collection has been initialized</returns>
        void InitCollection();

        bool CollectionExists();
    }
}