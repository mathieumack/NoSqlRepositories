using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoSqlRepositories.Core.NoSQLException
{
    public class IndexNotFoundNoSQLException : Exception
    {
        public IndexNotFoundNoSQLException() { }

        public IndexNotFoundNoSQLException(string message) : base(message) { }

        public IndexNotFoundNoSQLException(string message, Exception inner) : base(message, inner) { }
    }
}
