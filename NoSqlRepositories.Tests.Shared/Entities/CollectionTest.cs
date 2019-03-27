//using NoSqlRepositories.Core;
//using System;
//using System.Collections.Generic;

//namespace NoSqlRepositories.Test.Shared.Entities
//{
//    public class CollectionTest : IBaseEntity
//    {

//        /// <summary>
//        /// Unique indentifier
//        /// </summary>
//        public string Id { get; set; }

//        /// <summary>
//        /// Creation date of the object in the Viewer repository
//        /// </summary>
//        public DateTimeOffset SystemCreationDate { get; set; }

//        /// <summary>
//        /// Last update date of the object in the Viewer repository
//        /// </summary>
//        public DateTimeOffset SystemLastUpdateDate { get; set; }

//        /// <summary>
//        /// Logical delete status
//        /// </summary>
//        public bool Deleted { get; set; }


//        public List<TestEntity> PolymorphCollection { get; set; } = new List<TestEntity>();
//    }
//}