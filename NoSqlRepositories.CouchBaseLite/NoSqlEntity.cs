//using Couchbase.Lite;
//using Newtonsoft.Json;
//using Newtonsoft.Json.Linq;
//using NoSqlRepositories.Core;
//using NoSqlRepositories.Core.interfaces;
//using NoSqlRepositories.Utils;
//using System;
//using System.Collections.Generic;
//using System.Linq;

//namespace NoSqlRepositories.CouchBaseLite
//{
//    public class NoSqlEntity<T> : INoSqlEntity<T> where T : class, IBaseEntity
//    {
//        #region Couchbase document
        
//        internal NoSqlEntityDocument Document { get; private set; }

//        /// <summary>
//        /// Internal link to the entity model
//        /// This object is used to update base entity informations when the
//        /// repository do some changed on the object
//        /// </summary>
//        private T entityModel;

//        #endregion

//        public string Id {
//            get
//            {
//                if(Document != null && Document.Document != null)
//                    return Document.Document.Id;
//                else
//                    return Document.MutableDocument.Id;
//            }
//            set
//            {
//                SetString("Id", value);
//                if (entityModel != null)
//                    entityModel.Id = value;
//            }
//        }

//        /// <summary>
//        /// Creation date of the object in the repository
//        /// </summary>
//        public DateTimeOffset SystemCreationDate
//        {
//            get
//            {
//                return GetDate("SystemCreationDate");
//            }
//            set
//            {
//                SetDate("SystemCreationDate", value);
//                if (entityModel != null)
//                    entityModel.SystemCreationDate = value;
//            }
//        }

//        /// <summary>
//        /// Last update date of the object in the repository
//        /// </summary>
//        public DateTimeOffset SystemLastUpdateDate
//        {
//            get
//            {
//                return GetDate("SystemLastUpdateDate");
//            }
//            set
//            {
//                SetDate("SystemLastUpdateDate", value);
//                if (entityModel != null)
//                    entityModel.SystemLastUpdateDate = value;
//            }
//        }

//        private readonly string collectionName;
//        public string CollectionName
//        {
//            get
//            {
//                return collectionName;
//            }
//        }
        
//        /// <summary>
//        /// Used for existing documents
//        /// </summary>
//        /// <param name="collectionName"></param>
//        /// <param name="document"></param>
//        public NoSqlEntity(Database database, Document document)
//        {
//            this.collectionName = document.GetString("collection");
//            Document = new NoSqlEntityDocument(document);

//            database.AddDocumentChangeListener(document.Id, DocumentChanged);
//        }

//        /// <summary>
//        /// Allow you to create a new document
//        /// </summary>
//        /// <param name="collectionName"></param>
//        /// <param name="document"></param>
//        public NoSqlEntity(string collectionName, MutableDocument document)
//        {
//            this.collectionName = collectionName;
//            Document = new NoSqlEntityDocument(document);
            
//            // We update the collection field for the object :
//            document.SetString("collection", collectionName);

//            database.AddDocumentChangeListener(document.Id, DocumentChanged);
//        }

//        private void DocumentChanged(object sender, DocumentChangedEventArgs e)
//        {
//            if (Document.Document != null)
//                Document = new NoSqlEntityDocument(e.Database.GetDocument(e.DocumentID));
//            else
//            {
//                using(var databasedocument = )
//                Document = new NoSqlEntityDocument(e.Database.GetDocument(e.DocumentID));
//            }
//        }

//        #region Get values

//        public bool Contains(string key)
//        {
//            return Document.Document.Contains(key);
//        }

//        public bool GetBoolean(string key)
//        {
//            return Document.Document.GetBoolean(key);
//        }

//        public DateTimeOffset GetDate(string key)
//        {
//            return Document.Document.GetDate(key);
//        }

//        public double GetDouble(string key)
//        {
//            return Document.Document.GetDouble(key);
//        }

//        public float GetFloat(string key)
//        {
//            return Document.Document.GetFloat(key);
//        }

//        public int GetInt(string key)
//        {
//            return Document.Document.GetInt(key);
//        }

//        public long GetLong(string key)
//        {
//            return Document.Document.GetLong(key);
//        }

//        public string GetString(string key)
//        {
//            return Document.Document.GetString(key);
//        }

//        public object GetValue(string key)
//        {
//            return Document.Document.GetValue(key);
//        }

//        #endregion

//        #region Set values

//        public void Remove(string key)
//        {
//            Document.CheckMutable();
//            Document.MutableDocument.Remove(key);
//        }

//        public void SetBoolean(string key, bool value)
//        {
//            Document.CheckMutable();
//            Document.MutableDocument.SetBoolean(key, value);
//        }

//        public void SetDate(string key, DateTimeOffset value)
//        {
//            Document.CheckMutable();
//            Document.MutableDocument.SetDate(key, value);
//        }

//        public void SetDouble(string key, double value)
//        {
//            Document.CheckMutable();
//            Document.MutableDocument.SetDouble(key, value);
//        }

//        public void SetFloat(string key, float value)
//        {
//            Document.CheckMutable();
//            Document.MutableDocument.SetFloat(key, value);
//        }

//        public void SetInt(string key, int value)
//        {
//            Document.CheckMutable();
//            Document.MutableDocument.SetInt(key, value);
//        }

//        public void SetLong(string key, long value)
//        {
//            Document.CheckMutable();
//            Document.MutableDocument.SetLong(key, value);
//        }

//        public void SetString(string key, string value)
//        {
//            Document.CheckMutable();
//            Document.MutableDocument.SetString(key, value);
//        }

//        public void SetValue(string key, object value)
//        {
//            Document.CheckMutable();
//            Document.MutableDocument.SetValue(key, value);
//        }

//        #endregion

//        #region Domain entity mapping

//        public T GetEntityDomain()
//        {
//            var simpleDictionary = Document.Document.ToDictionary();
//            var fields = ObjectToDictionaryHelper.ListOfFields<T>();
//            var objectFields = new Dictionary<string, object>();
//            foreach(var field in fields)
//            {
//                if(simpleDictionary.ContainsKey(field))
//                    objectFields.Add(field, simpleDictionary[field]);
//            }
//            JObject obj = JObject.FromObject(objectFields);
//            return obj.ToObject<T>();
//        }

//        public void SetEntityDomain(T entityModel)
//        {
//            this.entityModel = entityModel;

//            var properties = ObjectToDictionaryHelper.ToDictionary(entityModel);
//            foreach(var prop in properties)
//            {
//                if (prop.Value is int)
//                    SetInt(prop.Key, (int)prop.Value);
//                if (prop.Value is long)
//                    SetLong(prop.Key, (long)prop.Value);
//                else if (prop.Value is bool)
//                    SetBoolean(prop.Key, (bool)prop.Value);
//                else if (prop.Value is DateTime)
//                {
//                    if ((DateTime)prop.Value != default(DateTime))
//                        SetDate(prop.Key, (DateTime)prop.Value);
//                }
//                else if (prop.Value is double)
//                    SetDouble(prop.Key, (double)prop.Value);
//                else if (prop.Value is float)
//                    SetFloat(prop.Key, (float)prop.Value);
//                else if (prop.Value is string)
//                    SetString(prop.Key, (string)prop.Value);
//                else
//                    SetValue(prop.Key, prop.Value);
//            }
//        }

//        #endregion
//    }
//}
