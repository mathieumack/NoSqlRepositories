using System.Collections.Generic;

namespace NoSqlRepositories.Core
{
    public class BulkInsertResult<TId> : Dictionary<TId, InsertResult>
    {
    }
}