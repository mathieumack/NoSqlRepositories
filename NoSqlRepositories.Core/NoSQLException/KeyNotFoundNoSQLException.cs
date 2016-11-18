using System;

namespace NoSqlRepositories.Core.NoSQLException
{
    public class KeyNotFoundNoSQLException : Exception
    {
        public KeyNotFoundNoSQLException() { }

        public KeyNotFoundNoSQLException(string message) : base(message) { }

        public KeyNotFoundNoSQLException(string message, Exception inner) : base(message, inner) { }
    }
}
