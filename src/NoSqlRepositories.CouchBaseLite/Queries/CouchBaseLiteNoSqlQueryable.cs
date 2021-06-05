using Couchbase.Lite.Query;
using Linq2CouchBaseLiteExpression;
using NoSqlRepositories.Core;
using NoSqlRepositories.Shared.Queries;
using System.Collections.Generic;
using System.Linq;

namespace NoSqlRepositories.CouchBaseLite.Queries
{
    internal class CouchBaseLiteNoSqlQueryable<T> : NoSqlQueryable<T> where T : class, IBaseEntity, new()
    {
        private readonly CouchBaseLiteRepository<T> repository;

        public CouchBaseLiteNoSqlQueryable(CouchBaseLiteRepository<T> repository)
        {
            this.repository = repository;
        }

        public override int Count()
        {
            return Select().Count();
        }

        public override IEnumerable<T> Select()
        {
            IExpression whereExpression = Expression.Property("collection").EqualTo(Expression.String(repository.GetCollectionName()));

            // Interpret Linq query to expression
            if (Filter != null)
            {
                var wherePreFilterExpression = Linq2CouchbaseLiteQueryExpression.GenerateFromExpression(Filter);
                if (wherePreFilterExpression != null)
                    whereExpression = whereExpression.And(wherePreFilterExpression);
            }

            IOrdering orderByExpression = Ordering.Property("SystemCreationDate").Ascending();

            var queryBuilder = QueryBuilder.Select(SelectResult.Expression(Meta.ID))
                                            .From(DataSource.Database(repository.Database))
                                            .Where(whereExpression)
                                            // add default ordering by creation date :
                                            .OrderBy(orderByExpression)
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
