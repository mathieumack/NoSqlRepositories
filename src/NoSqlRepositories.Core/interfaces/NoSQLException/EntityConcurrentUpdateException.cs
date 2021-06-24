using System;

namespace NoSqlRepositories.Core.NoSQLException
{
    public class EntityConcurrentUpdateException : Exception
    {
        public EntityConcurrentUpdateException() { }
        public EntityConcurrentUpdateException(string message) : base(message) { }
        public EntityConcurrentUpdateException(string message, Exception inner) : base(message, inner) { }
    }
}
