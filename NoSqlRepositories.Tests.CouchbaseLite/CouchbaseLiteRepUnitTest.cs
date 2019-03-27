using Microsoft.VisualStudio.TestTools.UnitTesting;
using NoSqlRepositories.CouchBaseLite;
using NoSqlRepositories.Test.Shared;
using NoSqlRepositories.Test.Shared.Entities;

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

            var entityRepo = new CouchBaseLiteRepository<TestEntity>(NoSQLCoreUnitTests.testContext.DeploymentDirectory, dbName);
            var entityRepo2 = new CouchBaseLiteRepository<TestEntity>(NoSQLCoreUnitTests.testContext.DeploymentDirectory, dbName);
            //var collectionEntityRepo = new CouchBaseLiteRepository<CollectionTest>(NoSQLCoreUnitTests.testContext.DeploymentDirectory, dbName);
            var entityExtraEltRepo = new CouchBaseLiteRepository<TestExtraEltEntity>(NoSQLCoreUnitTests.testContext.DeploymentDirectory, dbName);
            
            test = new NoSQLCoreUnitTests(entityRepo, entityRepo2, entityExtraEltRepo,
                NoSQLCoreUnitTests.testContext.DeploymentDirectory, dbName);
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
        public void CouchbaseLite_Count()
        {
            test.Count();
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
