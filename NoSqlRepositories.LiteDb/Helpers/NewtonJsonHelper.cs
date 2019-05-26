using Newtonsoft.Json;
using System;

namespace NoSqlRepositories.LiteDb.Helpers
{
    public static class NewtonJsonHelper
    {
        /// <summary>
        /// Perform a deep Copy of the object, using Json as a serialisation method.
        /// </summary>
        /// <typeparam name="T">The type of object being copied.</typeparam>
        /// <param name="source">The object instance to copy.</param>
        /// <param name="overhideDateTimeZone">Force timezone of datetime objects</param>
        /// <returns>The copied object.</returns>
        /// <summary>
        public static T CloneJson<T>(T source, DateTimeZoneHandling overhideDateTimeZone)
        {
            // Don't serialize a null object, simply return the default for that object
            if (Object.ReferenceEquals(source, null))
            {
                return default(T);
            }

            // initialize inner objects individually
            // for example in default constructor some list property initialized with some values,
            // but in 'source' these items are cleaned -
            // without ObjectCreationHandling.Replace default constructor values will be added to result
            var serializeSettings = new JsonSerializerSettings
            {
                ObjectCreationHandling = ObjectCreationHandling.Replace,
                TypeNameHandling = TypeNameHandling.Objects // If missing, polymorphism will not work
            };

            if (overhideDateTimeZone != default(DateTimeZoneHandling))
            {
                serializeSettings.DateTimeZoneHandling = overhideDateTimeZone;
            }

            var serialize = JsonConvert.SerializeObject(source, Formatting.None, serializeSettings);
            var dersizalize = JsonConvert.DeserializeObject<T>(serialize, serializeSettings);

            return dersizalize;
        }

        /// <summary>
        /// Perform a deep Copy of the object, using Json as a serialisation method.
        /// </summary>
        /// <typeparam name="T">The type of object being copied.</typeparam>
        /// <param name="source">The object instance to copy.</param>
        /// <returns>The copied object.</returns>
        public static T CloneJson<T>(T source)
        {
            return CloneJson<T>(source, default(DateTimeZoneHandling));
        }
    }
}
