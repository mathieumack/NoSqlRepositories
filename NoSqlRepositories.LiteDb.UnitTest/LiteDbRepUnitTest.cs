using Microsoft.VisualStudio.TestTools.UnitTesting;
using NoSqlRepositories.LiteDb;
using NoSqlRepositories.Tests.Shared;
using NoSqlRepositories.Tests.Shared.Entities;
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
            var dbName = "NoSQLTestDb";

            var entityRepo = new LiteDbRepository<TestEntity>(Directory.GetCurrentDirectory(), dbName);
            var entityRepo2 = new LiteDbRepository<TestEntity>(Directory.GetCurrentDirectory(), dbName);
            //var collectionEntityRepo = new JsonFileRepository<CollectionTest>(NoSQLCoreUnitTests.testContext.DeploymentDirectory, dbName);
            var entityExtraEltRepo = new LiteDbRepository<TestExtraEltEntity>(Directory.GetCurrentDirectory(), dbName);
            
            test = new NoSQLCoreUnitTests(entityRepo, entityRepo2, entityExtraEltRepo,
                Directory.GetCurrentDirectory(), dbName);
        }

        #endregion

        #region NoSQLCoreUnitTests test methods

        [TestMethod]
        public void LiteDb_DatabaseName()
        {
            test.DatabaseName();
        }

        [TestMethod]
        public void LiteDb_ExpireAt()
        {
            test.ExpireAt();
        }

        [TestMethod]
        public void LiteDb_DoQuery()
        {
            test.DoQuery();
        }

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
        public void LiteDb_Count()
        {
            test.Count();
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
