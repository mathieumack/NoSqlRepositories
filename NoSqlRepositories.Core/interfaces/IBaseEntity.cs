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
        DateTime SystemCreationDate { get; set; }

        /// <summary>
        /// Last update date of the object in the repository
        /// </summary>
        DateTime SystemLastUpdateDate { get; set; }

        /// <summary>
        /// Logical delete status
        /// </summary>
        bool Deleted { get; set; }
    }
}
