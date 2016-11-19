using Microsoft.VisualStudio.TestTools.UnitTesting;
using MvvmCross.Plugins.File;
using MvvmCross.Plugins.File.Wpf;
using MvvmCross.Test.Core;
using NoSqlRepositories.MvvX.JsonFiles.Pcl;
using NoSqlRepositories.Test.Shared;
using NoSqlRepositories.Test.Shared.Entities;
using System;

namespace NoSqlRepositories.Tests.MvvX
{
    [TestClass]
    public class JsonFileRepUnitTest : MvxIoCSupportingTest
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
            base.Setup();
            var dbName = "NoSQLTestDb";

            // Add Sqlite plugin register. Do it only for unit tests (https://github.com/CouchBaseLite/CouchBaseLite-lite-net/wiki/Error-Dictionary#cblcs0001)
            //CouchBaseLite.Lite.Storage.SystemSQLite.Plugin.Register();

            var entityRepo = new JsonFileRepository<TestEntity>(Ioc.Resolve<IMvxFileStore>(), dbName);
            var entityRepo2 = new JsonFileRepository<TestEntity>(Ioc.Resolve<IMvxFileStore>(), dbName);
            var collectionEntityRepo = new JsonFileRepository<CollectionTest>(Ioc.Resolve<IMvxFileStore>(), dbName);
            var entityExtraEltRepo = new JsonFileRepository<TestExtraEltEntity>(Ioc.Resolve<IMvxFileStore>(), dbName);
            
            test = new NoSQLCoreUnitTests(entityRepo, entityRepo2, entityExtraEltRepo, collectionEntityRepo,
                NoSQLCoreUnitTests.testContext.DeploymentDirectory);
        }

        protected override void AdditionalSetup()
        {
            base.AdditionalSetup();

            var fileStore = new MvxWpfFileStore(NoSQLCoreUnitTests.testContext.DeploymentDirectory + "\\");
            Ioc.RegisterSingleton<IMvxFileStore>(fileStore);
        }

        #endregion
        
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
