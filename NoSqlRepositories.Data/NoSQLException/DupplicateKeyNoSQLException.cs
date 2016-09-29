using System;

namespace NoSqlRepositories.Data.NoSQLException
{
    public class DupplicateKeyNoSQLException:Exception
    {
        public DupplicateKeyNoSQLException()
        { }

        public DupplicateKeyNoSQLException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
