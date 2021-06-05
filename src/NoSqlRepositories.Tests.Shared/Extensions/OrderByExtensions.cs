using Microsoft.VisualStudio.TestTools.UnitTesting;
using NoSqlRepositories.Core.Helpers;
using NoSqlRepositories.Tests.Shared;
using NoSqlRepositories.Tests.Shared.Entities;
using NoSqlRepositories.Tests.Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NoSqlRepositories.UnitTest.Shared.Extensions
{
    public static class OrderByExtensions
    {
        public static void OrderBy(this NoSQLCoreUnitTests test)
        {
            test.entityRepo.TruncateCollection();

            var entity1 = TestHelper.GetEntity1();
            var entity3 = TestHelper.GetEntity3();
            var entity4 = TestHelper.GetEntity4();

            test.entityRepo.InsertMany(new List<TestEntity>() { entity1, entity3, entity4 });

            // Now we will do some queries :
            var query = test.entityRepo.Query()
                                        .OrderBy(e => e.Name);
            Assert.AreEqual(3, query.Count(), "Query should contain three elements");

            var elements = query.Select().ToList();
            Assert.AreEqual(entity1.Name, elements[0].Name);
            Assert.AreEqual(entity4.Name, elements[1].Name);
            Assert.AreEqual(entity3.Name, elements[2].Name);
        }

        public static void OrderByDescending(this NoSQLCoreUnitTests test)
        {
            test.entityRepo.TruncateCollection();

            var entity1 = TestHelper.GetEntity1();
            var entity3 = TestHelper.GetEntity3();
            var entity4 = TestHelper.GetEntity4();

            test.entityRepo.InsertMany(new List<TestEntity>() { entity1, entity3, entity4 });

            // Now we will do some queries :
            var query = test.entityRepo.Query()
                                        .OrderByDescending((TestEntity e) => e.Name);
            Assert.AreEqual(3, query.Count(), "Query should contain three elements");

            var elements = query.Select().ToList();
            Assert.AreEqual(entity3.Name, elements[0].Name);
            Assert.AreEqual(entity4.Name, elements[1].Name);
            Assert.AreEqual(entity1.Name, elements[2].Name);
        }
    }
}
