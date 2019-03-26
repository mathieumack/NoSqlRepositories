using System;

namespace NoSqlRepositories.Core.interfaces
{
    /// <summary>
    /// Base interface for noSQL objects 
    /// </summary>
    public interface INoSqlEntity<T> where T : class, IBaseEntity
    {
        /// <summary>
        /// Unique identifier
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Name of the collection
        /// </summary>
        string CollectionName { get; }
        
        /// <summary>
        /// Return a POCO object from database values
        /// </summary>
        /// <returns></returns>
        T GetEntityDomain();

        /// <summary>
        /// Update values from the POCO object
        /// </summary>
        /// <param name="entityModel"></param>
        void SetEntityDomain(T entityModel);

        //#region Get values

        //bool Contains(string key);
        //bool GetBoolean(string key);
        //DateTime GetDate(string key);
        //double GetDouble(string key);
        //float GetFloat(string key);
        //int GetInt(string key);
        //long GetLong(string key);
        //string GetString(string key);
        //object GetValue(string key);

        //#endregion

        //#region Set values

        //void Remove(string key);
        //void SetBoolean(string key, bool value);
        //void SetDate(string key, DateTime value);
        //void SetDouble(string key, double value);
        //void SetFloat(string key, float value);
        //void SetInt(string key, int value);
        //void SetLong(string key, long value);
        //void SetString(string key, string value);
        //void SetValue(string key, object value);

        //#endregion
    }
}
