using NoSqlRepositories.Core;
using NoSqlRepositories.Shared.Queries;
using System.Collections.Generic;
using System.Linq;

namespace NoSqlRepositories.JsonFiles.Queries
{
    internal class JsonNoSqlQueryable<T> : NoSqlQueryable<T> where T : class, IBaseEntity, new()
    {
        private readonly JsonFileRepository<T> repository;

        public JsonNoSqlQueryable(JsonFileRepository<T> repository)
        {
            this.repository = repository;
        }

        public override int Count()
        {
            return Select().Count();
        }

        public override IEnumerable<T> Select()
        {
            var query = repository.LocalDb.Values.Select(e => e);

            // Filters :
            if (Filter != null)
            {
                var filterFunction = Filter.Compile();
                query = query.Where(e => filterFunction.Invoke(e));
            }

            query = query.OrderBy(e => e.SystemCreationDate);

            if (Skip > 0)
                query = query.Skip(Skip);
            if (Limit > 0)
                query = query.Take(Limit);

            return query;
        }
    }
}
