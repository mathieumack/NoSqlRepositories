//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using MvvmCross.Platform;
//using MvvmCross.Plugins.File;
//using MvvmCross.Plugins.File.Wpf;
//using NoSqlRepositories.Test.Shared;
//using NoSqlRepositories.Test.Shared.Entities;
//using System;
//using System.Collections.Generic;

//namespace NoSqlRepositories.Tests.MvvX
//{
//    [TestClass]
//    public class CouchBaseLiteRepUnitTest : MvvXNoSQLCoreUnitTest
//    {

//        #region Members

//        public static ICouchBaseLiteLite CouchBaseLiteLiteManager;

//        #endregion

//        #region Initialize & Clean

//        [ClassInitialize()]
//        public static new void ClassInitialize(TestContext testContext)
//        {
//            NoSQLCoreUnitTests.ClassInitialize(testContext);
//        }

//        [TestInitialize]
//        public void TestInitialize()
//        {
//            base.Setup();

//            // Instanciate the manager only 1 time
//            if(CouchBaseLiteLiteManager == null)
//            {
//                CouchBaseLiteLiteManager = Mvx.Resolve<ICouchBaseLiteLite>();
//            }

//            // Add Sqlite plugin register. Do it only for unit tests (https://github.com/CouchBaseLite/CouchBaseLite-lite-net/wiki/Error-Dictionary#cblcs0001)
//            //CouchBaseLite.Lite.Storage.SystemSQLite.Plugin.Register();

//            string dbName = "TestDb";

//            var entityRepoCBL = new CouchBaseLiteRepository<TestEntity>(CouchBaseLiteLiteManager, dbName);
//            this.entityRepo = entityRepoCBL;
//            var entityRepoCBL2 = new CouchBaseLiteRepository<TestEntity>(CouchBaseLiteLiteManager, dbName);
//            this.entityRepo2 = entityRepoCBL;
//            this.collectionEntityRepo = new CouchBaseLiteRepository<CollectionTest>(CouchBaseLiteLiteManager, dbName);
//            this.entityExtraEltRepo = new CouchBaseLiteRepository<TestExtraEltEntity>(CouchBaseLiteLiteManager, dbName);

//            // Define mapping for polymorphism
//            entityRepoCBL.PolymorphicTypes["TestExtraEltEntity"] = typeof(TestExtraEltEntity);
//            entityRepoCBL2.PolymorphicTypes["TestExtraEltEntity"] = typeof(TestExtraEltEntity);

//        }

//        protected override void AdditionalSetup()
//        {
//            base.AdditionalSetup();

//            var fileStore = getFileStore();
//            Ioc.RegisterSingleton<IMvxFileStore>(fileStore);
//            Ioc.RegisterSingleton<ICouchBaseLiteLite>(
//                () =>
//                {
//                    var cbl = new CouchBaseLiteLite();
//                    cbl.Initialize(NoSQLCoreUnitTests.testContext.DeploymentDirectory + "\\");
//                    return cbl;
//                }
//            ); 
//        }


//        #endregion

//        #region NoSQLCoreUnitTests test methods

//        [TestMethod]
//        public override void InsertEntity()
//        {
//            base.InsertEntity();
//        }

//        [TestMethod]
//        public override void DeleteEntity()
//        {
//            base.DeleteEntity();
//        }

//        [TestMethod]
//        public override void TimeZoneTest()
//        {
//            base.TimeZoneTest();
//        }

//        [TestMethod]
//        public override void InsertExtraEltEntity()
//        {
//            base.InsertExtraEltEntity();
//        }

//        //[TestMethod]
//        // Limitation : CouchBaseLite repository doesn't handle polymorphism in attribute's entity of type List, Dictionary...
//        public override void Polymorphism()
//        {
//            base.Polymorphism();
//        }

//        [TestMethod]
//        public void Attachments()
//        {
//            base.Attachments(Ioc.Resolve<IMvxFileStore>());
//        }

//        [TestMethod]
//        public override void GetAll()
//        {
//            base.GetAll();
//        }


//        [TestMethod]
//        public override void GetTests()
//        {
//            base.GetTests();
//        }

//        //[TestMethod, ExpectedException(typeof(CouchBaseLiteLiteConcurrentException), AllowDerivedTypes = true)]
//        [TestMethod]
//        public void ConcurrentAccess()
//        {
//            base.ConcurrentAccess(true);
//        }


//        [TestMethod]
//        public override void ViewTests()
//        {
//            ((CouchBaseLiteRepository<TestEntity>)entityRepo).CreateView<int>(nameof(TestEntity.NumberOfChildenInt), "1");
//            ((CouchBaseLiteRepository<TestEntity>)entityRepo).CreateView<string>(nameof(TestEntity.Cities), "1");

//            // Ensure that if we create a view two time that doen't raise an exception
//            ((CouchBaseLiteRepository<TestEntity>)entityRepo).CreateView<int>(nameof(TestEntity.NumberOfChildenInt), "1");

//            //CouchBaseLiteRepUnitTest.entityRepo.CreateView<string>(nameof(TestEntity.Cities), "1", true);
//            base.ViewTests();


//        }

//        #endregion

//        #region Private

//        private MvxWpfFileStore getFileStore()
//        {
//            return new MvxWpfFileStore(NoSQLCoreUnitTests.testContext.DeploymentDirectory + "\\");
//        }

//        #endregion


//    }
//}
