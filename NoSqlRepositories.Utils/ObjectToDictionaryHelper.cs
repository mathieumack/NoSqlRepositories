using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace NoSqlRepositories.Utils
{
    /// <summary>
    /// Original source : 
    /// </summary>
    public static class ObjectToDictionaryHelper
    {
        #region POCO to Dictionnary
        // Source : https://softwareproduction.eu/2018/02/28/fast-conversion-objects-dictionaries-c/

        private static readonly MethodInfo AddToDicitonaryMethod = typeof(IDictionary<string, object>).GetMethod("Add");
        private static readonly ConcurrentDictionary<Type, Func<object, IDictionary<string, object>>> Converters = new ConcurrentDictionary<Type, Func<object, IDictionary<string, object>>>();
        private static readonly ConstructorInfo DictionaryConstructor = typeof(Dictionary<string, object>).GetConstructors().FirstOrDefault(c => c.IsPublic && !c.GetParameters().Any());

        public static IDictionary<string, object> ToDictionary(this object obj) => obj == null ? null : Converters.GetOrAdd(obj.GetType(), o =>
        {
            var outputType = typeof(IDictionary<string, object>);
            var inputType = obj.GetType();
            var inputExpression = Expression.Parameter(typeof(object), "input");
            var typedInputExpression = Expression.Convert(inputExpression, inputType);
            var outputVariable = Expression.Variable(outputType, "output");
            var returnTarget = Expression.Label(outputType);
            var body = new List<Expression>
            {
                Expression.Assign(outputVariable, Expression.New(DictionaryConstructor))
            };
            body.AddRange(
                from prop in inputType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy)
                where prop.CanRead && (prop.PropertyType.IsPrimitive || prop.PropertyType == typeof(string))
                let getExpression = Expression.Property(typedInputExpression, prop.GetMethod)
                select Expression.Call(outputVariable, AddToDicitonaryMethod, Expression.Constant(prop.Name), getExpression));
            body.Add(Expression.Return(returnTarget, outputVariable));
            body.Add(Expression.Label(returnTarget, Expression.Constant(null, outputType)));

            var lambdaExpression = Expression.Lambda<Func<object, IDictionary<string, object>>>(
                Expression.Block(new[] { outputVariable }, body),
                inputExpression);

            return lambdaExpression.Compile();
        })(obj);

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
