using System;

namespace NoSqlRepositories.Core.NoSQLException
{
    public class IndexNotFoundNoSQLException : Exception
    {
        public IndexNotFoundNoSQLException() { }

        public IndexNotFoundNoSQLException(string message) : base(message) { }

        public IndexNotFoundNoSQLException(string message, Exception inner) : base(message, inner) { }
    }
}
