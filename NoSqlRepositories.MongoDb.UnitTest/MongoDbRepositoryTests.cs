using NoSqlRepositories.Tests.Shared;
using NoSqlRepositories.MongoDb;
using NoSqlRepositories.Tests.Shared.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Configuration;
using MongoDB.Bson.Serialization;
using NoSqlRepositories.Core;
using MongoDB.Bson.Serialization.IdGenerators;
using System.IO;

namespace NoSqlRepositories.Tests.MongoDb
{
    [TestClass]
    public class MongoDbRepositoryTests
    {
        private static void RegisterMongoMapping<T>() where T : IBaseEntity
        {
            BsonClassMap<T>.RegisterClassMap<T>(
                cm =>
                {
                    cm.AutoMap();
                    cm.MapIdMember(c => c.Id).SetIdGenerator(StringObjectIdGenerator.Instance);
                }
            );
        }

        private NoSQLCoreUnitTests test;

        private MongoDbRepository<TestEntity> entityRepo;
        private MongoDbRepository<TestEntity> entityRepo2;
        private MongoDbRepository<TestExtraEltEntity> entityExtraEltRepo;

        [ClassInitialize()]
        public static void ClassInitialize(TestContext testContext)
        {
            NoSQLCoreUnitTests.ClassInitialize(testContext);

            RegisterMongoMapping<TestEntity>();
        }

        [TestInitialize]
        public void TestInitialize()
        {
            var serviceUri = "mongodb://localhost:27017";
            var databaseName = "UnitTstsNoSqlRepo";

            entityRepo = new MongoDbRepository<TestEntity>(serviceUri, databaseName);
            entityRepo2 = new MongoDbRepository<TestEntity>(serviceUri, databaseName);
            entityExtraEltRepo = new MongoDbRepository<TestExtraEltEntity>(serviceUri, databaseName);

            // Define mapping for polymorphism
            //entityRepo.PolymorphicTypes["TestExtraEltEntity"] = typeof(TestExtraEltEntity);
            //entityRepo2.PolymorphicTypes["TestExtraEltEntity"] = typeof(TestExtraEltEntity);

            test = new NoSQLCoreUnitTests(entityRepo, entityRepo2, entityExtraEltRepo,
                Directory.GetCurrentDirectory(), databaseName);
        }

        #region NoSQLCoreUnitTests test methods

        [TestMethod]

        public void MongoDb_InsertEntity()
        {
            test.InsertEntity();
        }

        [TestMethod]

        public void MongoDb_DeleteEntity()
        {
            test.DeleteEntity();
        }

        [TestMethod]
        public void MongoDb_TimeZoneTest()
        {
            test.TimeZoneTest();
        }

        [TestMethod]
        public void MongoDb_InsertExtraEltEntity()
        {
            test.InsertExtraEltEntity();
        }

        [TestMethod]
        public void MongoDb_Attachments()
        {
            test.Attachments();
        }

        [TestMethod]
        public void MongoDb_GetAll()
        {
            test.GetAll();
        }


        [TestMethod]
        public void MongoDb_GetTests()
        {
            test.GetTests();
        }

        // Not supported for now
        //[TestMethod]
        public void MongoDb_ConcurrentAccess()
        {
            test.ConcurrentAccess(false);
        }

        // Not supported for now
        //[TestMethod]
        public void MongoDb_ViewTests()
        {
            test.ViewTests();
        }

        #endregion  
    }
}
