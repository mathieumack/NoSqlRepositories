
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MvvmCross.Platform;
using MvvmCross.Plugins.File;
using MvvmCross.Plugins.File.Wpf;
using MvvmCross.Test.Core;
using MvvX.Plugins.CouchBaseLite;
using MvvX.Plugins.CouchBaseLite.Platform;
using NoSqlRepositories.MvvX.CouchBaseLite.Pcl;
using NoSqlRepositories.Test.Shared;
using NoSqlRepositories.Test.Shared.Entities;
using NoSqlRepositories.Test.Shared.Extensions;
using NoSqlRepositories.Test.Shared.Helpers;
using System;

namespace NoSqlRepositories.Tests.MvvX
{
    [TestClass]
    public class CouchBaseLiteRepUnitTest : MvxIoCSupportingTest
    {

        private NoSQLCoreUnitTests test;
        private static TestContext testContext;
        private const string dbName = "nosqltestcbldb"; // Not uppercase for CouchBase lite db names

        private CouchBaseLiteRepository<TestEntity> entityRepo;
        private CouchBaseLiteRepository<TestEntity> entityRepo2;
        private CouchBaseLiteRepository<TestExtraEltEntity> entityExtraEltRepo;
        private CouchBaseLiteRepository<CollectionTest> collectionEntityRepo;


        #region Members

        public static ICouchBaseLite CouchBaseLiteLiteManager;

        #endregion

        #region Initialize & Clean
        
        [ClassInitialize()]
        public static void ClassInitialize(TestContext testContext)
        {
            CouchBaseLiteRepUnitTest.testContext = testContext;
            NoSQLCoreUnitTests.ClassInitialize(testContext);
        }


        [TestInitialize]
        public void TestInitialize()
        {
            base.Setup();

            // Instanciate the manager only 1 time
            if (CouchBaseLiteLiteManager == null)
            {
                CouchBaseLiteLiteManager = Mvx.Resolve<ICouchBaseLite>();
            }
            
            // Add Sqlite plugin register. Do it only for unit tests (https://github.com/CouchBaseLite/CouchBaseLite-lite-net/wiki/Error-Dictionary#cblcs0001)
            //CouchBaseLite.Lite.Storage.SystemSQLite.Plugin.Register();

            entityRepo = new CouchBaseLiteRepository<TestEntity>(CouchBaseLiteLiteManager, dbName);
            entityRepo2 = new CouchBaseLiteRepository<TestEntity>(CouchBaseLiteLiteManager, dbName);
            collectionEntityRepo = new CouchBaseLiteRepository<CollectionTest>(CouchBaseLiteLiteManager, dbName);
            entityExtraEltRepo = new CouchBaseLiteRepository<TestExtraEltEntity>(CouchBaseLiteLiteManager, dbName);

            // Define mapping for polymorphism
            entityRepo.PolymorphicTypes["TestExtraEltEntity"] = typeof(TestExtraEltEntity);
            entityRepo2.PolymorphicTypes["TestExtraEltEntity"] = typeof(TestExtraEltEntity);

            test = new NoSQLCoreUnitTests(entityRepo, 
                                            entityRepo2, 
                                            entityExtraEltRepo, 
                                            collectionEntityRepo,
                                            NoSQLCoreUnitTests.testContext.DeploymentDirectory,
                                            dbName);
        }

        protected override void AdditionalSetup()
        {
            base.AdditionalSetup();

            var fileStore = new MvxWpfFileStore(NoSQLCoreUnitTests.testContext.DeploymentDirectory + "\\");
            Ioc.RegisterSingleton<IMvxFileStore>(fileStore);

            Ioc.RegisterSingleton<ICouchBaseLite>(
                () =>
                {
                    var cbl = new CouchBaseLite();
                    cbl.Initialize(NoSQLCoreUnitTests.testContext.DeploymentDirectory + "\\DB");
                    return cbl;
                }
            );
        }

        #endregion

        #region NoSQLCoreUnitTests test methods

        [TestMethod]
        public void MvvX_CBLite_ExpireAt()
        {
            test.ExpireAt();
        }

        [TestMethod]
        public void MvvX_CBLite_CompactDatabase()
        {
            test.CompactDatabase();
        }

        [TestMethod]
        public void MvvX_CBLite_InsertEntity()
        {
            test.InsertEntity();
        }

        [TestMethod]

        public void MvvX_CBLite_DeleteEntity()
        {
            test.DeleteEntity();
        }

        [TestMethod]
        public void MvvX_CBLite_TimeZoneTest()
        {
            test.TimeZoneTest();
        }

        [TestMethod]
        public void MvvX_CBLite_InsertExtraEltEntity()
        {
            test.InsertExtraEltEntity();
        }

        //[TestMethod]
        // Limitation : couchtest repository doesn't handle polymorphism in attribute's entity of type List, Dictionary...
        public void MvvX_CBLite_Polymorphism()
        {
            test.Polymorphism();
        }

        [TestMethod]
        public void MvvX_CBLite_Attachments()
        {
            test.Attachments();
        }

        [TestMethod]
        public void MvvX_CBLite_GetAll()
        {
            test.GetAll();
        }


        [TestMethod]
        public void MvvX_CBLite_GetTests()
        {
            test.GetTests();
        }

        // Not supported for now
        [TestMethod]
        public void MvvX_CBLite_ConcurrentAccess()
        {
            test.ConcurrentAccess(false);
        }

        private void MvvX_CBLite_CreateViews(CouchBaseLiteRepository<TestEntity> entityRepo)
        {

            entityRepo.CreateView<int>(nameof(TestEntity.NumberOfChildenInt), "1");
            entityRepo.CreateView<string>(nameof(TestEntity.Cities), "1");
            // Ensure that if we create a view two time that doen't raise an exception
            entityRepo.CreateView<int>(nameof(TestEntity.NumberOfChildenInt), "1");
        }
        
        [TestMethod]
        public void MvvX_CBLite_ViewTests()
        {
            MvvX_CBLite_CreateViews(entityRepo);
            //CouchBaseLiteRepUnitTest.entityRepo.CreateView<string>(nameof(TestEntity.Cities), "1", true);
            test.ViewTests();
        }
        
        [TestMethod]
        public void MvvX_CBLite_ExistingViewTests()
        {
            testContext.DeployDirectory(@"Ressources\nosqltestcbldb.cblite2", @"ExistingRepo\nosqltestcbldb.cblite2");
            var existingRepoManager = new CouchBaseLite();
            existingRepoManager.Initialize(Path.Combine(NoSQLCoreUnitTests.testContext.DeploymentDirectory,"ExistingRepo"));
            
            entityRepo = new CouchBaseLiteRepository<TestEntity>(existingRepoManager, dbName);
            MvvX_CBLite_CreateViews(entityRepo);


            var res3 = entityRepo.GetAll();
            Assert.AreEqual(4, res3.Count, "Existing view TestEntity contains 5 docs");

            var res1 = entityRepo.GetByField<int>(nameof(TestEntity.NumberOfChildenInt), 0);
            Assert.AreEqual(2, res1.Count, "Existing view TestEntity-NumberOfChildenInt contains 2 docs");

            var res2 = entityRepo.GetByField<string>(nameof(TestEntity.Cities), "Andernos");
            Assert.AreEqual(1, res2.Count, "Existing view TestEntity-Cities contains 1 docs");



            var entity6 = new TestEntity
            {
                Id = "6",
                Birthday = new DateTime(1985, 08, 12, 0, 0, 0, DateTimeKind.Utc),
                IsAMan = true,
                Name = "Balan",
                PoidsFloat = 70.15F,
                PoidsDouble = 70.15,
                NumberOfChildenInt = 0,
                NumberOfChildenLong = 9999999999999999
            };

            entityRepo.InsertOne(entity6);

            var res4 = entityRepo.GetByField<int>(nameof(TestEntity.NumberOfChildenInt), 0);
            Assert.AreEqual(3, res4.Count, "Existing view TestEntity-NumberOfChildenInt contains 5 docs");

            var res5 = entityRepo.GetByField<string>(nameof(TestEntity.Cities), "Andernos");
            Assert.AreEqual(1, res5.Count, "Existing view TestEntity-Cities contains 1 docs");

            var res6 = entityRepo.GetAll();
            Assert.AreEqual(5, res6.Count, "Existing view TestEntity contains 5 docs");

        }

        #endregion

        [TestMethod]
        public void MvvX_CBLite_DatabaseName()
        {
            test.DatabaseName();
        }

        [TestMethod]
        public void MvvX_CBLite_NewBaseTests()
        {
            var dbFolderPath = Path.Combine(NoSQLCoreUnitTests.testContext.DeploymentDirectory, "DB2");
            var fileStore = new MvxWpfFileStore(dbFolderPath);

            // Ensure that the db file doesn't exists
            if(fileStore.FolderExists(dbFolderPath))
                fileStore.DeleteFolder(dbFolderPath, true);

            var cbl = new CouchBaseLite();
            cbl.Initialize(dbFolderPath);
            
            // Create a new db (the db file is missing)
            entityRepo = new CouchBaseLiteRepository<TestEntity>(CouchBaseLiteLiteManager, "DB2");

            Assert.AreEqual(false, entityRepo.Exist("123456"));
        }

        [TestMethod]
        public void MvvX_CBLite_Count()
        {
            test.Count();
        }

        #region Private

        private MvxWpfFileStore getFileStore()
        {
            return new MvxWpfFileStore(NoSQLCoreUnitTests.testContext.DeploymentDirectory + "\\");
        }

        #endregion


    }
}
