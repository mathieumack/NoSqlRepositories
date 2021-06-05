using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace NoSqlRepositories.Core.Queries
{
    /// <summary>
    /// Queryable object
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface INoSqlQueryable<T>
    {
        /// <summary>
        /// To be used for paging. Defined the number of elements that must be skipped
        /// </summary>
        /// <param name="skipCount"></param>
        /// <returns></returns>
        INoSqlQueryable<T> Skip(int skipCount);

        /// <summary>
        /// To be used for paging. Defined the number of elements that must be retreived
        /// </summary>
        /// <param name="takeCount"></param>
        /// <returns></returns>
        INoSqlQueryable<T> Take(int takeCount);

        /// <summary>
        /// Filter data. Define 
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        INoSqlQueryable<T> Where(Expression<Func<T, bool>> filter);

        /// <summary>
        /// Order data regarding a specific field on an entity
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        INoSqlQueryable<T> OrderBy<TKey>(Expression<Func<T, TKey>> filter);

        /// <summary>
        /// Order data descending regarding a specific field on an entity
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        INoSqlQueryable<T> OrderByDescending<TKey>(Expression<Func<T, TKey>> filter);

        /// <summary>
        /// Return the total number of elements
        /// </summary>
        /// <returns></returns>
        int Count();

        /// <summary>
        /// Return all elements
        /// </summary>
        /// <returns></returns>
        IEnumerable<T> Select();
    }
}
