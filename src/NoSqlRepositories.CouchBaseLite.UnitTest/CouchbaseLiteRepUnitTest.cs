﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using NoSqlRepositories.CouchBaseLite;
using NoSqlRepositories.Tests.Shared;
using NoSqlRepositories.Tests.Shared.Entities;
using System.IO;
using NoSqlRepositories.UnitTest.Shared.Extensions;

namespace NoSqlRepositories.Tests.CouchbaseLite
{
    [TestClass]
    public class CouchbaseLiteRepUnitTest
    {
        private NoSQLCoreUnitTests test;

        #region Initialize & Clean

        [ClassInitialize()]
        public static void ClassInitialize(TestContext testContext)
        {
            NoSQLCoreUnitTests.ClassInitialize(testContext);
        }

        [TestInitialize]
        public void TestInitialize()
        {
            var dbName = "NoSQLTestDb";

            // Add Sqlite plugin register. Do it only for unit tests (https://github.com/CouchBaseLite/CouchBaseLite-lite-net/wiki/Error-Dictionary#cblcs0001)
            //CouchBaseLite.Lite.Storage.SystemSQLite.Plugin.Register();
            Couchbase.Lite.Support.NetDesktop.Activate();

            var entityRepo = new CouchBaseLiteRepository<TestEntity>(Directory.GetCurrentDirectory(), dbName);
            var entityRepo2 = new CouchBaseLiteRepository<TestEntity>(Directory.GetCurrentDirectory(), dbName);
            //var collectionEntityRepo = new CouchBaseLiteRepository<CollectionTest>(Directory.GetCurrentDirectory(), dbName);
            var entityExtraEltRepo = new CouchBaseLiteRepository<TestExtraEltEntity>(Directory.GetCurrentDirectory(), dbName);
            
            test = new NoSQLCoreUnitTests(entityRepo, entityRepo2, entityExtraEltRepo, Directory.GetCurrentDirectory(), dbName);
        }

        #endregion

        #region NoSQLCoreUnitTests test methods

        [TestMethod]
        public void CouchbaseLite_DatabaseName()
        {
            test.DatabaseName();
        }

        //[TestMethod]
        //public void CouchbaseLite_ExpireAt()
        //{
        //    test.ExpireAt();
        //}

        [TestMethod]
        public void CouchbaseLite_CompactDatabase()
        {
            test.CompactDatabase();
        }

        [TestMethod]

        public void CouchbaseLite_InsertEntity()
        {
            test.InsertEntity();
        }

        [TestMethod]

        public void CouchbaseLite_DeleteEntity()
        {
            test.DeleteEntity();
        }

        [TestMethod]
        public void CouchbaseLite_TimeZoneTest()
        {
            test.TimeZoneTest();
        }

        //Polymorphisme is not correctly managed yet
        [TestMethod]
        public void CouchbaseLite_InsertExtraEltEntity()
        {
            test.InsertExtraEltEntity();
        }


        [TestMethod]
        public void CouchbaseLite_Attachments()
        {
            test.Attachments();
        }

        [TestMethod]
        public void CouchbaseLite_GetIds()
        {
            test.GetIds();
        }

        [TestMethod]
        public void CouchbaseLite_GetAll()
        {
            test.GetAll();
        }


        [TestMethod]
        public void CouchbaseLite_GetTests()
        {
            test.GetTests();
        }

        [TestMethod]
        public void CouchbaseLite_DoQuery()
        {
            test.DoQuery();
        }

        [TestMethod]
        public void CouchbaseLite_DoQueryPaging()
        {
            test.DoQuery_Paging();
        }

        [TestMethod]
        public void CouchbaseLite_DoQueryWithOrdering()
        {
            test.Query_WithOrdering();
        }

        [TestMethod]
        public void CouchbaseLite_DoQueryv2()
        {
            test.Query();
        }

        [TestMethod]
        public void CouchbaseLite_DoQueryv2_Paging()
        {
            test.Query_Paging();
        }

        [TestMethod]
        public void CouchbaseLite_DoQueryv2_WithOrdering()
        {
            test.Query_WithOrdering();
        }

        [TestMethod]
        public void CouchbaseLite_OrderBy()
        {
            test.OrderBy();
        }

        [TestMethod]
        public void CouchbaseLite_OrderByDescending()
        {
            test.OrderByDescending();
        }

        [TestMethod]
        public void CouchbaseLite_Count()
        {
            test.Count();
        }

        [TestMethod]
        public void CouchbaseLite_PreFilter()
        {
            test.Filter();
        }

        [TestMethod]
        public void CouchbaseLite_FilterComplex()
        {
            test.FilterComplex();
        }

        [TestMethod]
        public void CouchbaseLite_FilterComplex_Contains()
        {
            test.FilterComplex_Contains();
        }

        // Not supported for now
        //[TestMethod]
        public void CouchbaseLite_ConcurrentAccess()
        {
            test.ConcurrentAccess(false);
        }

        // Not supported for now
        //[TestMethod]
        public void CouchbaseLite_ViewTests()
        {
            test.ViewTests();
        }

        #endregion
    }
}
