using System;
using System.Linq.Expressions;

namespace NoSqlRepositories.Core.Queries
{
    public static class QueryCreator
    {
        /// <summary>
        /// Create a new NoSqlQuery object to be used on Query
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="limit"></param>
        /// <param name="skip"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static NoSqlQuery<T> CreateQueryOptions<T>(int limit,
                                                            int skip,
                                                            Expression<Func<T, bool>> filter) 
                            where T : class, IBaseEntity, new()
        {
            return new NoSqlQuery<T>()
            {
                Limit = limit,
                Skip = skip,
                Filter = filter
            };
        }
    }
}
