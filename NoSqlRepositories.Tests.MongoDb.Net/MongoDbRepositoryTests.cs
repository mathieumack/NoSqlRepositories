using System;
using NoSqlRepositories.Test.Shared;
using NoSqlRepositories.MongoDb.Net;
using NoSqlRepositories.Test.Shared.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Configuration;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization;
using NoSqlRepositories.Core;
using MongoDB.Bson.Serialization.IdGenerators;

namespace NoSqlRepositories.Tests.MongoDb.Net
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
        private MongoDbRepository<CollectionTest> collectionEntityRepo;

        [ClassInitialize()]
        public static void ClassInitialize(TestContext testContext)
        {
            NoSQLCoreUnitTests.ClassInitialize(testContext);

            RegisterMongoMapping<TestEntity>();
            RegisterMongoMapping<CollectionTest>();
        }

        [TestInitialize]
        public void TestInitialize()
        {
            var dbName = "NoSQLTestMongoDb";

            entityRepo = new MongoDbRepository<TestEntity>(ConfigurationManager.AppSettings["mongoDB:ServiceUri"], ConfigurationManager.AppSettings["mongoDB:DatabaseName"]);
            entityRepo2 = new MongoDbRepository<TestEntity>(ConfigurationManager.AppSettings["mongoDB:ServiceUri"], ConfigurationManager.AppSettings["mongoDB:DatabaseName"]);
            collectionEntityRepo = new MongoDbRepository<CollectionTest>(ConfigurationManager.AppSettings["mongoDB:ServiceUri"], ConfigurationManager.AppSettings["mongoDB:DatabaseName"]);
            entityExtraEltRepo = new MongoDbRepository<TestExtraEltEntity>(ConfigurationManager.AppSettings["mongoDB:ServiceUri"], ConfigurationManager.AppSettings["mongoDB:DatabaseName"]);

            // Define mapping for polymorphism
            //entityRepo.PolymorphicTypes["TestExtraEltEntity"] = typeof(TestExtraEltEntity);
            //entityRepo2.PolymorphicTypes["TestExtraEltEntity"] = typeof(TestExtraEltEntity);

            test = new NoSQLCoreUnitTests(entityRepo, entityRepo2, entityExtraEltRepo, collectionEntityRepo,
                NoSQLCoreUnitTests.testContext.DeploymentDirectory, ConfigurationManager.AppSettings["mongoDB:DatabaseName"]);
        }

        #region NoSQLCoreUnitTests test methods

        [TestMethod]

        public void InsertEntity()
        {
            test.InsertEntity();
        }

        [TestMethod]

        public void DeleteEntity()
        {
            test.DeleteEntity();
        }

        [TestMethod]
        public void TimeZoneTest()
        {
            test.TimeZoneTest();
        }

        [TestMethod]
        public void InsertExtraEltEntity()
        {
            test.InsertExtraEltEntity();
        }

        //[TestMethod]
        // Limitation : couchtest repository doesn't handle polymorphism in attribute's entity of type List, Dictionary...
        public void Polymorphism()
        {
            test.Polymorphism();
        }

        [TestMethod]
        public void Attachments()
        {
            test.Attachments();
        }

        [TestMethod]
        public void GetAll()
        {
            test.GetAll();
        }


        [TestMethod]
        public void GetTests()
        {
            test.GetTests();
        }

        // Not supported for now
        //[TestMethod]
        public void ConcurrentAccess()
        {
            test.ConcurrentAccess(false);
        }

        // Not supported for now
        //[TestMethod]
        public void ViewTests()
        {
            test.ViewTests();
        }

        #endregion  
    }
}
