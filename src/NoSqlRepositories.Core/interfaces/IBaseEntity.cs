using System;

namespace NoSqlRepositories.Core
{
    /// <summary>
    /// Base interface for noSQL objects 
    /// </summary>
    public interface IBaseEntity
    {
        /// <summary>
        /// Unique identifier
        /// </summary>
        string Id { get; set; }

        /// <summary>
        /// Creation date of the object in the repository
        /// </summary>
        DateTimeOffset SystemCreationDate { get; set; }

        /// <summary>
        /// Last update date of the object in the repository
        /// </summary>
        DateTimeOffset SystemLastUpdateDate { get; set; }

        /// <summary>
        /// Logical delete status
        /// </summary>
        bool Deleted { get; set; }
    }
}
