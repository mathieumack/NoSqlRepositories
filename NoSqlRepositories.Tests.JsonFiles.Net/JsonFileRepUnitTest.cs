using Microsoft.VisualStudio.TestTools.UnitTesting;
using NoSqlRepositories.JsonFiles.Net;
using NoSqlRepositories.Test.Shared;
using NoSqlRepositories.Test.Shared.Entities;

namespace NoSqlRepositories.Tests.MvvX
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
            //CouchBaseLite.Lite.Storage.SystemSQLite.Plugin.Register();

            var entityRepo = new JsonFileRepository<TestEntity>(NoSQLCoreUnitTests.testContext.DeploymentDirectory, dbName);
            var entityRepo2 = new JsonFileRepository<TestEntity>(NoSQLCoreUnitTests.testContext.DeploymentDirectory, dbName);
            var collectionEntityRepo = new JsonFileRepository<CollectionTest>(NoSQLCoreUnitTests.testContext.DeploymentDirectory, dbName);
            var entityExtraEltRepo = new JsonFileRepository<TestExtraEltEntity>(NoSQLCoreUnitTests.testContext.DeploymentDirectory, dbName);
            
            test = new NoSQLCoreUnitTests(entityRepo, entityRepo2, entityExtraEltRepo, collectionEntityRepo,
                NoSQLCoreUnitTests.testContext.DeploymentDirectory, dbName);
        }

        #endregion

        #region NoSQLCoreUnitTests test methods

        [TestMethod]
        public void MvvX_JsonFiles_DatabaseName()
        {
            test.DatabaseName();
        }

        [TestMethod]
        public void MvvX_JsonFiles_ExpireAt()
        {
            test.ExpireAt();
        }

        [TestMethod]
        public void MvvX_JsonFiles_CompactDatabase()
        {
            test.CompactDatabase();
        }

        [TestMethod]

        public void MvvX_JsonFiles_InsertEntity()
        {
            test.InsertEntity();
        }

        [TestMethod]

        public void MvvX_JsonFiles_DeleteEntity()
        {
            test.DeleteEntity();
        }

        [TestMethod]
        public void MvvX_JsonFiles_TimeZoneTest()
        {
            test.TimeZoneTest();
        }

        [TestMethod]
        public void MvvX_JsonFiles_InsertExtraEltEntity()
        {
            test.InsertExtraEltEntity();
        }

        //[TestMethod]
        // Limitation : couchtest repository doesn't handle polymorphism in attribute's entity of type List, Dictionary...
        public void MvvX_JsonFiles_Polymorphism()
        {
            test.Polymorphism();
        }

        [TestMethod]
        public void MvvX_JsonFiles_Attachments()
        {
            test.Attachments();
        }

        [TestMethod]
        public void MvvX_JsonFiles_GetAll()
        {
            test.GetAll();
        }


        [TestMethod]
        public void MvvX_JsonFiles_GetTests()
        {
            test.GetTests();
        }

        [TestMethod]
        public void Mvvx_JsonFiles_Count()
        {
            test.Count();
        }

        // Not supported for now
        //[TestMethod]
        public void MvvX_JsonFiles_ConcurrentAccess()
        {
            test.ConcurrentAccess(false);
        }

        // Not supported for now
        //[TestMethod]
        public void MvvX_JsonFiles_ViewTests()
        {
            test.ViewTests();
        }

        #endregion  
    }
}
