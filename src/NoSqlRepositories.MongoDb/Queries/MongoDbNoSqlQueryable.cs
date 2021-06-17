using MongoDB.Driver;
using MongoDB.Driver.Linq;
using NoSqlRepositories.Core;
using NoSqlRepositories.Core.Queries;
using NoSqlRepositories.Shared.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace NoSqlRepositories.MongoDb.Queries
{
    internal class MongoDbNoSqlQueryable<T> : NoSqlQueryable<T> where T : class, IBaseEntity, new()
    {
        private readonly MongoDbRepository<T> repository;
        private bool ordered;
        private IMongoQueryable<T> query; 

        public MongoDbNoSqlQueryable(MongoDbRepository<T> repository)
        {
            this.repository = repository;
            this.query = repository.Collection.AsQueryable();
        }

        /// <inheritdoc/>
        public override int Count()
        {
            return Select().Count();
        }

        /// <inheritdoc/>
        public override INoSqlQueryable<T> Where(Expression<Func<T, bool>> filter)
        {
            query = query.Where(filter);

            return this;
        }

        /// <inheritdoc/>
        public override INoSqlQueryable<T> OrderBy<TKey>(Expression<Func<T, TKey>> filter)
        {
            ordered = true;

            query = query.OrderBy(filter);

            return this;
        }

        /// <inheritdoc/>
        public override INoSqlQueryable<T> OrderByDescending<TKey>(Expression<Func<T, TKey>> filter)
        {
            ordered = true;

            query = query.OrderByDescending(filter);

            return this;
        }

        /// <inheritdoc/>
        public override IEnumerable<T> Select()
        {
            // Default order :
            if(!ordered)
                query = query.OrderBy(e => e.SystemCreationDate);

            return query.Select(e => e);
        }
    }
}
