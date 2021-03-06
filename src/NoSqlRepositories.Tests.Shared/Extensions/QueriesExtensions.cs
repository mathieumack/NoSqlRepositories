﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using NoSqlRepositories.Core.Helpers;
using NoSqlRepositories.Tests.Shared;
using NoSqlRepositories.Tests.Shared.Entities;
using NoSqlRepositories.Tests.Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NoSqlRepositories.UnitTest.Shared.Extensions
{
    public static class QueriesExtensions
    {
        public static void Query(this NoSQLCoreUnitTests test)
        {
            test.entityRepo.TruncateCollection();

            var entity1 = TestHelper.GetEntity1();
            var entity3 = TestHelper.GetEntity3();
            var entity4 = TestHelper.GetEntity4();

            test.entityRepo.InsertMany(new List<TestEntity>() { entity1, entity3, entity4 });

            // Now we will do some queries :
            var query = test.entityRepo.Query()
                                    .Take(2)
                                    .Skip(0);
            Assert.AreEqual(2, query.Count(), "Query should contain three elements");

            // Now we will add a filter method :
            query = test.entityRepo.Query()
                                    .Where((filterItem) => filterItem.NumberOfChildenInt == 0);
            Assert.AreEqual(1, query.Count(), "Query should contain two elements");
        }

        public static void Query_Paging(this NoSQLCoreUnitTests test)
        {
            test.entityRepo.TruncateCollection();

            NoSQLRepoHelper.DateTimeUtcNow = (() => new DateTimeOffset(DateTime.UtcNow));

            var entity1 = TestHelper.GetEntity1();
            var entity2 = TestHelper.GetEntity2();
            var entity3 = TestHelper.GetEntity3();
            var entity4 = TestHelper.GetEntity4();

            // add height entries
            test.entityRepo.InsertMany(new List<TestEntity>() { entity1, entity2, entity3, entity4 });

            // Now we will do some queries :
            var query = test.entityRepo.Query()
                                    .Take(2)
                                    .Skip(2);
            Assert.AreEqual(2, query.Count(), "Query should contain two element");

            // Now we will add a filter method :
            var elements = query.Select().ToList();
            Assert.AreEqual(entity3.Name, elements[0].Name);
            Assert.AreEqual(entity4.Name, elements[1].Name);

            // Reset DateTimeUtcNow function
            var now = new DateTime(2016, 01, 01, 0, 0, 0, DateTimeKind.Utc);
            NoSQLRepoHelper.DateTimeUtcNow = (() => now);
        }

        public static void Query_WithOrdering(this NoSQLCoreUnitTests test)
        {
            test.entityRepo.TruncateCollection();

            NoSQLRepoHelper.DateTimeUtcNow = (() => new DateTimeOffset(DateTime.UtcNow));

            var entity1 = TestHelper.GetEntity1();
            var entity3 = TestHelper.GetEntity3();
            var entity4 = TestHelper.GetEntity4();

            test.entityRepo.InsertOne(entity1);
            test.entityRepo.InsertOne(entity3);
            test.entityRepo.InsertOne(entity4);

            // Now we will do some queries :
            var query = test.entityRepo.Query()
                                        .Take(3)
                                        .OrderBy(e => e.Name)
                                        .Skip(0);
            Assert.AreEqual(3, query.Count(), "Query should contain three elements");

            var elements = query.Select().ToList();

            Assert.AreEqual(entity1.Name, elements[0].Name);
            Assert.AreEqual(entity4.Name, elements[1].Name);
            Assert.AreEqual(entity3.Name, elements[2].Name);

            // Reset DateTimeUtcNow function
            var now = new DateTime(2016, 01, 01, 0, 0, 0, DateTimeKind.Utc);
            NoSQLRepoHelper.DateTimeUtcNow = (() => now);
        }
    }
}
