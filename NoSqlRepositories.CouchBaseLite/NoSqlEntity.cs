using Couchbase.Lite;
using NoSqlRepositories.Core;
using NoSqlRepositories.Core.interfaces;
using NoSqlRepositories.Utils;
using System;

namespace NoSqlRepositories.CouchBaseLite
{
    public class NoSqlEntity<T> : INoSqlEntity<T> where T : class, IBaseEntity
    {
        #region Couchbase document
        
        internal NoSqlEntityDocument Document { get; private set; }

        #endregion

        public string Id {
            get
            {
                if(Document != null)
                    return Document.Document.Id;
                else
                    return Document.MutableDocument.Id;
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
        /// Used for existing documents
        /// </summary>
        /// <param name="collectionName"></param>
        /// <param name="document"></param>
        public NoSqlEntity(Document document)
        {
            this.collectionName = document.GetString("collection");
            Document = new NoSqlEntityDocument(document);
        }

        /// <summary>
        /// Allow you to create a new document
        /// </summary>
        /// <param name="collectionName"></param>
        /// <param name="document"></param>
        public NoSqlEntity(string collectionName, MutableDocument document)
        {
            this.collectionName = collectionName;
            Document = new NoSqlEntityDocument(document);
            
            // We update the collection field for the object :
            document.SetString("collection", collectionName);
        }

        #region Get values

        public bool Contains(string key)
        {
            return Document.Document.Contains(key);
        }

        public bool GetBoolean(string key)
        {
            return Document.Document.GetBoolean(key);
        }

        public DateTime GetDate(string key)
        {
            return Document.Document.GetDate(key).Date;
        }

        public double GetDouble(string key)
        {
            return Document.Document.GetDouble(key);
        }

        public float GetFloat(string key)
        {
            return Document.Document.GetFloat(key);
        }

        public int GetInt(string key)
        {
            return Document.Document.GetInt(key);
        }

        public long GetLong(string key)
        {
            return Document.Document.GetLong(key);
        }

        public string GetString(string key)
        {
            return Document.Document.GetString(key);
        }

        public object GetValue(string key)
        {
            return Document.Document.GetValue(key);
        }

        #endregion

        #region Set values

        public void Remove(string key)
        {
            Document.CheckMutable();
            Document.MutableDocument.Remove(key);
        }

        public void SetBoolean(string key, bool value)
        {
            Document.CheckMutable();
            Document.MutableDocument.SetBoolean(key, value);
        }

        public void SetDate(string key, DateTime value)
        {
            Document.CheckMutable();
            Document.MutableDocument.SetDate(key, value);
        }

        public void SetDouble(string key, double value)
        {
            Document.CheckMutable();
            Document.MutableDocument.SetDouble(key, value);
        }

        public void SetFloat(string key, float value)
        {
            Document.CheckMutable();
            Document.MutableDocument.SetFloat(key, value);
        }

        public void SetInt(string key, int value)
        {
            Document.CheckMutable();
            Document.MutableDocument.SetInt(key, value);
        }

        public void SetLong(string key, long value)
        {
            Document.CheckMutable();
            Document.MutableDocument.SetLong(key, value);
        }

        public void SetString(string key, string value)
        {
            Document.CheckMutable();
            Document.MutableDocument.SetString(key, value);
        }

        public void SetValue(string key, object value)
        {
            Document.CheckMutable();
            Document.MutableDocument.SetValue(key, value);
        }

        #endregion

        #region Domain entity mapping

        public T GetEntityDomain()
        {
            var simpleDictionary = Document.Document.ToDictionary();
            return ObjectToDictionaryHelper.DictionaryToObject<T>(simpleDictionary);
        }

        public void SetEntityDomain(T entityModel)
        {
            var properties = ObjectToDictionaryHelper.ToDictionary(entityModel);
            foreach(var prop in properties)
            {
                if (prop.Value is int)
                    SetInt(prop.Key, (int)prop.Value);
                if (prop.Value is long)
                    SetLong(prop.Key, (long)prop.Value);
                else if (prop.Value is bool)
                    SetBoolean(prop.Key, (bool)prop.Value);
                else if (prop.Value is DateTime)
                    SetDate(prop.Key, (DateTime)prop.Value);
                else if (prop.Value is double)
                    SetDouble(prop.Key, (double)prop.Value);
                else if (prop.Value is float)
                    SetFloat(prop.Key, (float)prop.Value);
                else if (prop.Value is string)
                    SetString(prop.Key, (string)prop.Value);
                else
                    SetValue(prop.Key, prop.Value);
            }
        }

        #endregion
    }
}
