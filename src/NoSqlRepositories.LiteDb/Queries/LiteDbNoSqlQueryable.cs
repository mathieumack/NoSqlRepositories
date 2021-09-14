using LiteDB;
using NoSqlRepositories.Core;
using NoSqlRepositories.Core.Queries;
using NoSqlRepositories.Shared.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace NoSqlRepositories.LiteDb.Queries
{
    internal class LiteDbNoSqlQueryable<T> : NoSqlQueryable<T> where T : class, IBaseEntity, new()
    {
        private readonly LiteCollection<T> repository;
        private bool ordered;
        private Expression<Func<T, bool>> whereCondition;

        public LiteDbNoSqlQueryable(LiteCollection<T> repository)
        {
            this.repository = repository;
        }

        /// <inheritdoc/>
        public override int Count()
        {
            return Select().Count();
        }

        /// <inheritdoc/>
        public override INoSqlQueryable<T> Where(Expression<Func<T, bool>> filter)
        {
            this.whereCondition = filter;

            return this;
        }

        /// <inheritdoc/>
        public override INoSqlQueryable<T> OrderBy<TKey>(Expression<Func<T, TKey>> filter)
        {
            ordered = true;

            // Not implemented yet cf : https://github.com/mbdavid/LiteDB/issues/805
            // TODO : Manual operation - add a custom Where ?

            return this;
        }

        /// <inheritdoc/>
        public override INoSqlQueryable<T> OrderByDescending<TKey>(Expression<Func<T, TKey>> filter)
        {
            ordered = true;

            // Not implemented yet cf : https://github.com/mbdavid/LiteDB/issues/805
            // TODO : Manual operation - add a custom Where ?

            return this;
        }

        /// <inheritdoc/>
        public override IEnumerable<T> Select()
        {
            return repository.Find(whereCondition, Skip, Limit);
        }
    }
}
