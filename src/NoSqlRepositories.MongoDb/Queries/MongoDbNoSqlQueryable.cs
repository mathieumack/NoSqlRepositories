using MongoDB.Driver;
using MongoDB.Driver.Linq;
using NoSqlRepositories.Core;
using NoSqlRepositories.Shared.Queries;
using System.Collections.Generic;
using System.Linq;

namespace NoSqlRepositories.MongoDb.Queries
{
    internal class MongoDbNoSqlQueryable<T> : NoSqlQueryable<T> where T : class, IBaseEntity, new()
    {
        private readonly MongoDbRepository<T> repository;

        public MongoDbNoSqlQueryable(MongoDbRepository<T> repository)
        {
            this.repository = repository;
        }

        public override int Count()
        {
            return Select().Count();
        }

        public override IEnumerable<T> Select()
        {
            var queryable = repository.Collection.AsQueryable();

            if (Filter != null)
                queryable = queryable.Where(Filter);

            // Default order :
            var orderedQueryable = queryable.OrderBy(e => e.SystemCreationDate);

            return orderedQueryable.Select(e => e);
        }
    }
}
