using System;

namespace NoSqlRepositories.LiteDb
{
    internal class ExpirationEntry
    {
        /// <summary>
        /// Id of the document
        /// </summary>
        public string Id { get; set; }

        public DateTimeOffset? ExpirationDate { get; set; }
    }
}
