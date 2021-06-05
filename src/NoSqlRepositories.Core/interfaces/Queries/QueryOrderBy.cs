using System;
using System.Linq.Expressions;

namespace NoSqlRepositories.Core.Queries
{
    public class QueryOrderBy<T> where T : IBaseEntity
    {
        /// <summary>
        /// Ascending order by description
        /// Must be null if <see cref="OrderByDescendingFieldName"/> is set
        /// </summary>
        public Expression<Func<T, int>> OrderByFieldName { get; }

        /// <summary>
        /// Descending order by description
        /// Must be null if <see cref="OrderByFieldName"/> is set
        /// </summary>
        public Expression<Func<T, int>> OrderByDescendingFieldName { get; }
    }
}
