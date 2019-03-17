using System;

namespace NoSqlRepositories.Core.NoSQLException
{
    public class QueryNotAcknowledgedException : Exception
    {
        public QueryNotAcknowledgedException()
        { }

        public QueryNotAcknowledgedException(string message, Exception innerException) 
            :base(message, innerException)
        { }
    }
}
