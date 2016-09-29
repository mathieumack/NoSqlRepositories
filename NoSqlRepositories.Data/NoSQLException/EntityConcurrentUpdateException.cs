using System;

namespace NoSqlRepositories.Data.NoSQLException
{
    public class EntityConcurrentUpdateException : Exception
    {
        public EntityConcurrentUpdateException() { }
        public EntityConcurrentUpdateException(string message) : base(message) { }
        public EntityConcurrentUpdateException(string message, Exception inner) : base(message, inner) { }
    }
}
