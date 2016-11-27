using System;
using System.Collections.Generic;

namespace NoSqlRepositories.JsonFiles.Net
{
    internal class DbConfiguration
    {
        private IDictionary<string, DateTime?> documentExpirations;

        public DbConfiguration()
        {
            documentExpirations = new Dictionary<string, DateTime?>();
        }

        /// <summary>
        /// Remove a document by his id
        /// </summary>
        /// <param name="id"></param>
        public void Delete(string id)
        {
            if (documentExpirations.ContainsKey(id))
                documentExpirations.Remove(id);
        }

        /// <summary>
        /// Delete expired documents from database
        /// </summary>
        public void Compact()
        {
            foreach(var key in documentExpirations.Keys)
            {
                if (documentExpirations[key].HasValue
                    && documentExpirations[key].Value < DateTime.Now)
                    Delete(key);
            }
        }

        /// <summary>
        /// Check if a document is expired
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool IsExpired(string id)
        {
            return documentExpirations.ContainsKey(id) 
                    && documentExpirations[id].HasValue
                    && documentExpirations[id].Value < DateTime.Now;
        }

        /// <summary>
        /// Define an expiration date for a document
        /// </summary>
        /// <param name="id">id of the document</param>
        /// <param name="expirationDate">expiration date, or null to remove expiration</param>
        public void ExpireAt(string id, DateTime? expirationDate)
        {
            if (documentExpirations.ContainsKey(id))
                documentExpirations[id] = expirationDate;
            else
                documentExpirations.Add(id, expirationDate);
        }

        public void TruncateCollection()
        {
            documentExpirations.Clear();
        }
    }
}
