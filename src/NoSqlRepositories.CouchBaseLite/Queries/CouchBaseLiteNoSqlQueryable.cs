using Couchbase.Lite.Query;
using Linq2CouchBaseLiteExpression;
using NoSqlRepositories.Core;
using NoSqlRepositories.Core.Queries;
using NoSqlRepositories.Shared.Queries;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NoSqlRepositories.CouchBaseLite.Queries
{
    internal class CouchBaseLiteNoSqlQueryable<T> : NoSqlQueryable<T> where T : class, IBaseEntity, new()
    {
        private readonly CouchBaseLiteRepository<T> repository;
        private readonly List<IOrdering> ordering = new List<IOrdering>();
        private bool ordered;
        private IExpression whereExpression;

        public CouchBaseLiteNoSqlQueryable(CouchBaseLiteRepository<T> repository)
        {
            this.repository = repository;
            // Default where filter for Couchbase lite :
            whereExpression = Expression.Property("collection").EqualTo(Expression.String(repository.GetCollectionName()));
        }

        public override int Count()
        {
            return Select().Count();
        }

        /// <inheritdoc/>
        public override INoSqlQueryable<T> Where(System.Linq.Expressions.Expression<Func<T, bool>> filter)
        {
            var wherePreFilterExpression = Linq2CouchbaseLiteQueryExpression.GenerateFromExpression(filter);
            if (wherePreFilterExpression != null)
                whereExpression = whereExpression.And(wherePreFilterExpression);

            return this;
        }

        public override INoSqlQueryable<T> OrderBy<TKey>(System.Linq.Expressions.Expression<Func<T, TKey>> filter)
        {
            ordered = true;

            var orderingExpression = Linq2CouchbaseLiteOrderingExpression.GenerateFromExpression(filter, true);
            if (orderingExpression != null)
                ordering.Add(orderingExpression);

            return this;
        }

        public override INoSqlQueryable<T> OrderByDescending<TKey>(System.Linq.Expressions.Expression<Func<T, TKey>> filter)
        {
            ordered = true;

            var orderingExpression = Linq2CouchbaseLiteOrderingExpression.GenerateFromExpression(filter, false);
            if (orderingExpression != null)
                ordering.Add(orderingExpression);

            return this;
        }

        public override IEnumerable<T> Select()
        {
            if(!ordered)
                ordering.Add(Ordering.Property("SystemCreationDate").Ascending());

            var queryBuilder = QueryBuilder.Select(SelectResult.Expression(Meta.ID))
                                            .From(DataSource.Database(repository.Database))
                                            .Where(whereExpression)
                                            // add default ordering by creation date :
                                            .OrderBy(ordering.ToArray())
                                            .Limit(Limit > 0 ? Expression.Int(Limit + Skip) : Expression.Int(int.MaxValue));

            IList<string> ids = null;
            using (var query = queryBuilder)
            {
                ids = query.Execute().Skip(Skip).Select(row => row.GetString("id")).ToList();
            }

            var resultSet = ids.Select(e => repository.GetById(e));

            return resultSet;
        }
    }
}
