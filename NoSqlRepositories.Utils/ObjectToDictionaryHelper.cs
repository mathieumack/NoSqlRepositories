using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;

namespace NoSqlRepositories.Utils
{
    /// <summary>
    /// Original source : 
    /// </summary>
    public static class ObjectToDictionaryHelper
    {
        #region POCO to Dictionnary
        // Source : https://gist.github.com/jarrettmeyer/798667/a87f9bcac2ec68541511f17da3c244c0e05bdc49

        public static IDictionary<string, object> ToDictionary(this object source)
        {
            return source.ToDictionary<object>();
        }

        public static IList<string> ListOfFields<T>()
        {
            var result = new List<string>();
            foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(typeof(T)))
            {
                result.Add(property.Name);
            }
            return result;
        }

        public static IDictionary<string, object> ToDictionary<T>(this object source)
        {
            if (source == null) ThrowExceptionWhenSourceArgumentIsNull();

            var dictionary = new Dictionary<string, object>();
            foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(source))
            {
                object value = property.GetValue(source);
                // TODO : Add management of Extra elements in a next release
                //if(property.Attributes.OfType<JsonExtensionDataAttribute>().Any() && value is Dictionary<string, object>)
                //{
                //    var dict = value as Dictionary<string, object>;
                //    foreach(var key in dict.Keys)
                //    {
                //        if(!dictionary.ContainsKey(key))
                //            dictionary.Add(key, dict[key]);
                //    }
                //}
                //else
                //{
                    dictionary.Add(property.Name, value);
                //}
            }
            return dictionary;
        }

        private static bool IsOfType<T>(object value)
        {
            return value is T;
        }

        private static void ThrowExceptionWhenSourceArgumentIsNull()
        {
            throw new NullReferenceException("Unable to convert anonymous object to a dictionary. The source anonymous object is null.");
        }

        #endregion

        #region Dictionary to POCO
        // Source : https://dejanstojanovic.net/aspnet/2016/december/dictionary-to-object-in-c/

        public static T DictionaryToObject<T>(IDictionary<string, object> dictionary) where T : class
        {
            return DictionaryToObject(dictionary) as T;
        }

        private static dynamic DictionaryToObject(IDictionary<string, object> dictionary)
        {
            var expandoObj = new ExpandoObject();
            var expandoObjCollection = (ICollection<KeyValuePair<string, object>>)expandoObj;

            foreach (var keyValuePair in dictionary)
            {
                expandoObjCollection.Add(keyValuePair);
            }
            dynamic eoDynamic = expandoObj;
            return eoDynamic;
        }

        #endregion
    }
}
