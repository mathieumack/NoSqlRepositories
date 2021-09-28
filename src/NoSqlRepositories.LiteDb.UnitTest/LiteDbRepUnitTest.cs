using Microsoft.VisualStudio.TestTools.UnitTesting;
using NoSqlRepositories.LiteDb;
using NoSqlRepositories.Tests.Shared;
using NoSqlRepositories.Tests.Shared.Entities;
using NoSqlRepositories.UnitTest.Shared.Extensions;
using System;
using System.IO;

namespace NoSqlRepositories.Tests.LiteDb
{
    [TestClass]
    public class LiteDbRepUnitTest
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
            var dbName = "testDb";

            var entityRepo = new LiteDbRepository<TestEntity>(Directory.GetCurrentDirectory(), dbName);
            //var entityRepo2 = new LiteDbRepository<TestEntity>(Directory.GetCurrentDirectory(), dbName);
            //var entityExtraEltRepo = new LiteDbRepository<TestExtraEltEntity>(Directory.GetCurrentDirectory(), dbName);
            
            test = new NoSQLCoreUnitTests(entityRepo, null, null, Directory.GetCurrentDirectory(), dbName);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            test.CleanUp();
        }

        #endregion

        #region NoSQLCoreUnitTests test methods

        [TestMethod]
        public void LiteDb_DatabaseName()
        {
            test.DatabaseName();
        }

        //[TestMethod]
        //public void LiteDb_ExpireAt()
        //{
        //    test.ExpireAt();
        //}

        [TestMethod]
        public void LiteDb_CompactDatabase()
        {
            test.CompactDatabase();
        }

        [TestMethod]

        public void LiteDb_InsertEntity()
        {
            test.InsertEntity();
        }

        [TestMethod]

        public void LiteDb_DeleteEntity()
        {
            test.DeleteEntity();
        }

        [TestMethod]
        public void LiteDb_TimeZoneTest()
        {
            test.TimeZoneTest();
        }

        //Polymorphisme is not correctly managed yet
        [TestMethod]
        public void LiteDb_InsertExtraEltEntity()
        {
            test.InsertExtraEltEntity();
        }


        [TestMethod]
        public void LiteDb_Attachments()
        {
            test.Attachments();
        }

        [TestMethod]
        public void LiteDb_GetIds()
        {
            test.GetIds();
        }

        [TestMethod]
        public void LiteDb_GetAll()
        {
            test.GetAll();
        }


        [TestMethod]
        public void LiteDb_GetTests()
        {
            test.GetTests();
        }

        [TestMethod]
        [ExpectedException(typeof(NotImplementedException))]
        public void LiteDb_DoQuery()
        {
            test.DoQuery();
        }

        [TestMethod]
        public void LiteDb_DoQueryPaging()
        {
            test.DoQuery_Paging();
        }

        [TestMethod]
        public void LiteDb_DoQueryWithOrdering()
        {
            test.Query_WithOrdering();
        }

        [TestMethod]
        public void LiteDb_DoQueryv2()
        {
            test.Query();
        }

        [TestMethod]
        public void LiteDb_DoQueryv2_Paging()
        {
            test.Query_Paging();
        }

        [TestMethod]
        public void LiteDb_DoQueryv2_WithOrdering()
        {
            test.Query_WithOrdering();
        }

        [TestMethod]
        public void LiteDb_OrderBy()
        {
            test.OrderBy();
        }

        [TestMethod]
        public void LiteDb_OrderByDescending()
        {
            test.OrderByDescending();
        }

        [TestMethod]
        public void LiteDb_Count()
        {
            test.Count();
        }

        [TestMethod]
        public void LiteDb_PreFilter()
        {
            test.Filter();
        }

        [TestMethod]
        public void LiteDb_FilterComplex()
        {
            test.FilterComplex();
        }

        [TestMethod]
        public void LiteDb_FilterComplex_Contains()
        {
            test.FilterComplex_Contains();
        }

        // Not supported for now
        //[TestMethod]
        public void LiteDb_ConcurrentAccess()
        {
            test.ConcurrentAccess(false);
        }

        // Not supported for now
        //[TestMethod]
        public void LiteDb_ViewTests()
        {
            test.ViewTests();
        }

        #endregion
    }
}
