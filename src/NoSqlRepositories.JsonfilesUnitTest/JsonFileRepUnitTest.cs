using Microsoft.VisualStudio.TestTools.UnitTesting;
using NoSqlRepositories.JsonFiles;
using NoSqlRepositories.Tests.Shared;
using NoSqlRepositories.Tests.Shared.Entities;
using System.IO;

namespace NoSqlRepositories.Tests.JsonFiles
{
    [TestClass]
    public class JsonFileRepUnitTest
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

            var entityRepo = new JsonFileRepository<TestEntity>(Directory.GetCurrentDirectory(), dbName);
            var entityRepo2 = new JsonFileRepository<TestEntity>(Directory.GetCurrentDirectory(), dbName);
            var entityExtraEltRepo = new JsonFileRepository<TestExtraEltEntity>(Directory.GetCurrentDirectory(), dbName);
            
            test = new NoSQLCoreUnitTests(entityRepo, entityRepo2, entityExtraEltRepo,
                Directory.GetCurrentDirectory(), dbName);
        }

        #endregion

        #region NoSQLCoreUnitTests test methods

        [TestMethod]
        public void JsonFiles_DatabaseName()
        {
            test.DatabaseName();
        }

        [TestMethod]
        public void JsonFiles_ExpireAt()
        {
            test.ExpireAt();
        }

        [TestMethod]
        public void JsonFiles_DoQuery()
        {
            test.DoQuery();
        }

        [TestMethod]
        public void JsonFiles_CompactDatabase()
        {
            test.CompactDatabase();
        }

        [TestMethod]

        public void JsonFiles_InsertEntity()
        {
            test.InsertEntity();
        }

        [TestMethod]

        public void JsonFiles_DeleteEntity()
        {
            test.DeleteEntity();
        }

        [TestMethod]
        public void JsonFiles_TimeZoneTest()
        {
            test.TimeZoneTest();
        }

        [TestMethod]
        public void JsonFiles_InsertExtraEltEntity()
        {
            test.InsertExtraEltEntity();
        }
        
        [TestMethod]
        public void JsonFiles_Attachments()
        {
            test.Attachments();
        }

        [TestMethod]
        public void JsonFiles_GetIds()
        {
            test.GetIds();
        }

        [TestMethod]
        public void JsonFiles_GetAll()
        {
            test.GetAll();
        }


        [TestMethod]
        public void JsonFiles_GetTests()
        {
            test.GetTests();
        }

        [TestMethod]
        public void JsonFiles_Count()
        {
            test.Count();
        }

        [TestMethod]
        public void JsonFiles_FilterComplex()
        {
            test.FilterComplex();
        }

        [TestMethod]
        public void JsonFiles_FilterComplex_Contains()
        {
            test.FilterComplex_Contains();
        }

        // Not supported for now
        //[TestMethod]
        public void JsonFiles_ConcurrentAccess()
        {
            test.ConcurrentAccess(false);
        }

        // Not supported for now
        //[TestMethod]
        public void JsonFiles_ViewTests()
        {
            test.ViewTests();
        }

        #endregion  
    }
}
