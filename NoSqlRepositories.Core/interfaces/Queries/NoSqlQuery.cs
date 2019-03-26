using System;

namespace NoSqlRepositories.Core.Queries
{
    public class NoSqlQuery<T> where T : class, IBaseEntity, new()
    {        
        /// <summary>
        /// Gets or sets the maximum number of rows to return. 
        /// The default value is int.MaxValue, meaning 'unlimited'.
        /// </summary>
        public int Limit { get; set; }

        /// <summary>
        /// Gets or sets an optional predicate that filters the resulting query rows.
        /// If present, it's called on every row returned from the query, and if it returnsfalseNO
        /// the row is skipped.
        /// </summary>
        public Func<T, bool> PostFilter { get; set; }

        /// <summary>
        /// Gets or sets the number of initial rows to skip. Default value is 0.
        /// </summary>
        /// <value>
        /// The number of initial rows to skip. Default value is 0
        /// </value>
        public int Skip { get; set; }
    }
}
