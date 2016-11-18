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
//    public class CouchbaseRepUnitTest : MvvXNoSQLCoreUnitTest
//    {

//        #region Members

//        public static ICouchBaseLite couchBaseLiteManager;

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
//            if(couchBaseLiteManager == null)
//            {
//                couchBaseLiteManager = Mvx.Resolve<ICouchBaseLite>();
//            }

//            // Add Sqlite plugin register. Do it only for unit tests (https://github.com/couchbase/couchbase-lite-net/wiki/Error-Dictionary#cblcs0001)
//            //Couchbase.Lite.Storage.SystemSQLite.Plugin.Register();

//            string dbName = "TestDb";

//            var entityRepoCBL = new CouchBaseRepository<TestEntity>(couchBaseLiteManager, dbName);
//            this.entityRepo = entityRepoCBL;
//            var entityRepoCBL2 = new CouchBaseRepository<TestEntity>(couchBaseLiteManager, dbName);
//            this.entityRepo2 = entityRepoCBL;
//            this.collectionEntityRepo = new CouchBaseRepository<CollectionTest>(couchBaseLiteManager, dbName);
//            this.entityExtraEltRepo = new CouchBaseRepository<TestExtraEltEntity>(couchBaseLiteManager, dbName);

//            // Define mapping for polymorphism
//            entityRepoCBL.PolymorphicTypes["TestExtraEltEntity"] = typeof(TestExtraEltEntity);
//            entityRepoCBL2.PolymorphicTypes["TestExtraEltEntity"] = typeof(TestExtraEltEntity);

//        }

//        protected override void AdditionalSetup()
//        {
//            base.AdditionalSetup();

//            var fileStore = getFileStore();
//            Ioc.RegisterSingleton<IMvxFileStore>(fileStore);
//            Ioc.RegisterSingleton<ICouchBaseLite>(
//                () =>
//                {
//                    var cbl = new CouchBaseLite();
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
//        // Limitation : couchbase repository doesn't handle polymorphism in attribute's entity of type List, Dictionary...
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

//        //[TestMethod, ExpectedException(typeof(CouchbaseLiteConcurrentException), AllowDerivedTypes = true)]
//        [TestMethod]
//        public void ConcurrentAccess()
//        {
//            base.ConcurrentAccess(true);
//        }


//        [TestMethod]
//        public override void ViewTests()
//        {
//            ((CouchBaseRepository<TestEntity>)entityRepo).CreateView<int>(nameof(TestEntity.NumberOfChildenInt), "1");
//            ((CouchBaseRepository<TestEntity>)entityRepo).CreateView<string>(nameof(TestEntity.Cities), "1");

//            // Ensure that if we create a view two time that doen't raise an exception
//            ((CouchBaseRepository<TestEntity>)entityRepo).CreateView<int>(nameof(TestEntity.NumberOfChildenInt), "1");

//            //CouchbaseRepUnitTest.entityRepo.CreateView<string>(nameof(TestEntity.Cities), "1", true);
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
