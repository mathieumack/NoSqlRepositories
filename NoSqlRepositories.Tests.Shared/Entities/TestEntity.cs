using NoSqlRepositories.Core;
using System;
using System.Collections.Generic;

namespace NoSqlRepositories.Tests.Shared.Entities
{
    public class TestEntity : IBaseEntity
    {
        /// <summary>
        /// Unique indentifier
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Creation date of the object in the Viewer repository
        /// </summary>
        public DateTimeOffset SystemCreationDate { get; set; }

        /// <summary>
        /// Last update date of the object in the Viewer repository
        /// </summary>
        public DateTimeOffset SystemLastUpdateDate { get; set; }

        /// <summary>
        /// Logical delete status
        /// </summary>
        public bool Deleted { get; set; }

        public string Name { get; set; }
        public DateTimeOffset Birthday { get; set; }
        public float PoidsFloat { get; set; }
        public double PoidsDouble { get; set; }
        public bool IsAMan { get; set; }
        public int NumberOfChildenInt { get; set; }
        public long NumberOfChildenLong { get; set; }

        public List<string> Cities { get; set; } = new List<string>();

        public List<TestEntity> Childs { get; set; } = new List<TestEntity>();
    }
}