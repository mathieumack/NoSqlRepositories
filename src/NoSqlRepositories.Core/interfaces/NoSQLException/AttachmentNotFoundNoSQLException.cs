﻿using System;

namespace NoSqlRepositories.Core.NoSQLException
{
    public class AttachmentNotFoundNoSQLException : Exception
    {
        public AttachmentNotFoundNoSQLException() { }

        public AttachmentNotFoundNoSQLException(string message) : base(message) { }

        public AttachmentNotFoundNoSQLException(string message, Exception inner) : base(message, inner) { }
    }
}
