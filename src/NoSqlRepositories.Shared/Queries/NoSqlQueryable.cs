using NoSqlRepositories.Core;
using NoSqlRepositories.Core.Queries;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace NoSqlRepositories.Shared.Queries
{
    public abstract class NoSqlQueryable<T> : INoSqlQueryable<T> where T : class, IBaseEntity, new()
    {
        /// <summary>
        /// Gets or sets the number of initial rows to skip. Default value is 0.
        /// </summary>
        /// <value>
        /// The number of initial rows to skip. Default value is 0
        /// </value>
        protected int Skip { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of rows to return. 
        /// The default value is int.MaxValue, meaning 'unlimited'.
        /// </summary>
        protected int Limit { get; set; }

        /// <summary>
        /// Gets or sets an optional predicate that filters the resulting query rows.
        /// If present, it's called on every row returned from the query, and if it returnsfalseNO
        /// the row is skipped.
        /// </summary>
        protected Expression<Func<T, bool>> Filter { get; set; }

        /// <inheritdoc/>
        public INoSqlQueryable<T> Take(int takeCount)
        {
            this.Limit = takeCount;

            // Used in order to be able to create Fluent
            return this;
        }

        /// <inheritdoc/>
        INoSqlQueryable<T> INoSqlQueryable<T>.Skip(int skipCount)
        {
            this.Skip = skipCount;

            // Used in order to be able to create Fluent
            return this;
        }

        /// <inheritdoc/>
        public INoSqlQueryable<T> Where(Expression<Func<T, bool>> filter)
        {
            this.Filter = filter;

            // Used in order to be able to create Fluent
            return this;
        }

        /// <inheritdoc/>
        public abstract int Count();

        /// <inheritdoc/>
        public abstract IEnumerable<T> Select();
    }
}
