using System;

namespace NoSqlRepositories.Core
{
    public interface INoSQLDB
    {
        /// <summary>
        /// Type of the current engine
        /// </summary>
        NoSQLEngineType EngineType { get; }

        /// <summary>
        /// Get database name used for configuration
        /// </summary>
        string DatabaseName { get; }

        /// <summary>
        /// Change the active database
        /// </summary>
        /// <param name="dbName"></param>
        void UseDatabase(string dbName);

        /// <summary>
        /// Delete temporary items (expired, logical deleted, ...)
        /// Do not works with JsonFiles repositories yet
        /// </summary>
        /// <returns>True if the refresh if ok, false if there is an error</returns>
        bool CompactDatabase();

        /// <summary>
        /// Sets an absolute point in time for the document to expire. Must be a DateTime 
        /// in the future. Pass a null value to cancel an expiration.
        /// This item will be deleted after a date with the CompactDatabase() method
        /// Do not works with JsonFiles repositories yet, only CouchBaseLite
        /// </summary>
        /// <param name="id">Id of the document</param>
        /// <param name="dateLimit">Limit date, can be null to delete a previous date</param>
        void ExpireAt(string id, DateTime? dateLimit);

        long TruncateCollection();

        void DropCollection();

        void SetCollectionName(string typeName);

        string GetCollectionName();

        /// <summary>
        /// Run initilization command of the collection (if required by the repository implementation)
        /// </summary>
        /// <returns>True if the collection has been initialized</returns>
        void InitCollection();

        /// <summary>
        /// Check if the collection exists.
        /// </summary>
        /// <param name="createIfNotExists">If true, the collection will be created if it not already exists</param>
        /// <returns>True if the collection exists, else false</returns>
        bool CollectionExists(bool createIfNotExists);
    }
}