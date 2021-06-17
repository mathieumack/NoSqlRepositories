using NoSqlRepositories.Core;
using NoSqlRepositories.Core.Queries;
using NoSqlRepositories.Shared.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace NoSqlRepositories.JsonFiles.Queries
{
    internal class JsonNoSqlQueryable<T> : NoSqlQueryable<T> where T : class, IBaseEntity, new()
    {
        private readonly JsonFileRepository<T> repository;
        private IEnumerable<T> query;
        private bool ordered;

        public JsonNoSqlQueryable(JsonFileRepository<T> repository)
        {
            this.repository = repository;
            query = repository.LocalDb.Values.Select(e => e);
        }

        /// <inheritdoc/>
        public override int Count()
        {
            return Select().Count();
        }

        /// <inheritdoc/>
        public override INoSqlQueryable<T> Where(Expression<Func<T, bool>> filter)
        {
            var filterFunction = filter.Compile();
            query = query.Where(e => filterFunction.Invoke(e));

            return this;
        }

        /// <inheritdoc/>
        public override INoSqlQueryable<T> OrderBy<TKey>(Expression<Func<T, TKey>> filter)
        {
            ordered = true;

            var filterFunction = filter.Compile();
            query = query.OrderBy(filterFunction);

            return this;
        }

        /// <inheritdoc/>
        public override INoSqlQueryable<T> OrderByDescending<TKey>(Expression<Func<T, TKey>> filter)
        {
            ordered = true;

            var filterFunction = filter.Compile();
            query = query.OrderByDescending(filterFunction);

            return this;
        }

        /// <inheritdoc/>
        public override IEnumerable<T> Select()
        {
            // Copy must be done as If we call Count() after a select (or before), the filtering will failed (Skip / Limit)
            var result = query;

            if(!ordered)
                result = result.OrderBy(e => e.SystemCreationDate);

            if (Skip > 0)
                result = result.Skip(Skip);
            if (Limit > 0)
                result = result.Take(Limit);

            return result;
        }
    }
}
