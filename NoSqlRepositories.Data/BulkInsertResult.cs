using System.Collections.Generic;

namespace NoSqlRepositories.Data
{
    public class BulkInsertResult<TId> : Dictionary<TId, InsertResult>
    {
    }
}