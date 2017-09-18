using System;
using NoSqlRepositories.Test.Shared;
using NoSqlRepositories.AzureDocumentDb.Net;
using NoSqlRepositories.Test.Shared.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Configuration;
using System.Threading.Tasks;

namespace NoSqlRepositories.Tests.AzureDocumentDb.Net
{
    [TestClass]
    public class AsyncAzureDocumentDbRepositoryTests
    {
        private AsyncNoSQLCoreUnitTests test;

        private AsyncAzureDocumentDbRepository<TestEntity> entityRepo;
        private AsyncAzureDocumentDbRepository<TestEntity> entityRepo2;
        private AsyncAzureDocumentDbRepository<TestExtraEltEntity> entityExtraEltRepo;
        private AsyncAzureDocumentDbRepository<CollectionTest> collectionEntityRepo;

        [ClassInitialize()]
        public static void ClassInitialize(TestContext testContext)
        {
            NoSQLCoreUnitTests.ClassInitialize(testContext);
        }

        [TestInitialize]
        public async Task TestInitialize()
        {
            var dbName = "NoSQLTestAzureDb";

            entityRepo = new AsyncAzureDocumentDbRepository<TestEntity>(ConfigurationManager.AppSettings["endPoint"], ConfigurationManager.AppSettings["primaryKey"]);
            await entityRepo.UseDatabase(dbName);
            entityRepo2 = new AsyncAzureDocumentDbRepository<TestEntity>(ConfigurationManager.AppSettings["endPoint"], ConfigurationManager.AppSettings["primaryKey"]);
            await entityRepo2.UseDatabase(dbName);
            collectionEntityRepo = new AsyncAzureDocumentDbRepository<CollectionTest>(ConfigurationManager.AppSettings["endPoint"], ConfigurationManager.AppSettings["primaryKey"]);
            await collectionEntityRepo.UseDatabase(dbName);
            entityExtraEltRepo = new AsyncAzureDocumentDbRepository<TestExtraEltEntity>(ConfigurationManager.AppSettings["endPoint"], ConfigurationManager.AppSettings["primaryKey"]);
            await entityExtraEltRepo.UseDatabase(dbName);

            // Define mapping for polymorphism
            //entityRepo.PolymorphicTypes["TestExtraEltEntity"] = typeof(TestExtraEltEntity);
            //entityRepo2.PolymorphicTypes["TestExtraEltEntity"] = typeof(TestExtraEltEntity);

            test = new AsyncNoSQLCoreUnitTests(entityRepo, entityRepo2, entityExtraEltRepo, collectionEntityRepo,
                NoSQLCoreUnitTests.testContext.DeploymentDirectory);
        }

        #region NoSQLCoreUnitTests test methods

        [TestMethod]

        public async Task InsertEntity()
        {
            await test.InsertEntity();
        }

        [TestMethod]

        public async Task DeleteEntity()
        {
            await test.DeleteEntity();
        }

        [TestMethod]
        public async Task TimeZoneTest()
        {
            await test.TimeZoneTest();
        }

        [TestMethod]
        public async Task InsertExtraEltEntity()
        {
            await test.InsertExtraEltEntity();
        }

        //[TestMethod]
        // Limitation : couchtest repository doesn't handle polymorphism in attribute's entity of type List, Dictionary...
        public async Task Polymorphism()
        {
            await test.Polymorphism();
        }

        [TestMethod]
        public async Task Attachments()
        {
            await test.Attachments();
        }

        [TestMethod]
        public async Task GetAll()
        {
            await test.GetAll();
        }


        [TestMethod]
        public async Task GetTests()
        {
            await test.GetTests();
        }

        // Not supported for now
        //[TestMethod]
        public async Task ConcurrentAccess()
        {
            await test.ConcurrentAccess(false);
        }

        // Not supported for now
        //[TestMethod]
        public async Task ViewTests()
        {
            await test.ViewTests();
        }

        #endregion  
    }
}
