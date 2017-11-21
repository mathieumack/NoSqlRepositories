using Microsoft.VisualStudio.TestTools.UnitTesting;
using MvvmCross.Platform;
using MvvX.Plugins.CouchBaseLite;
using NoSqlRepositories.MvvX.CouchBaseLite.Pcl;
using NoSqlRepositories.Test.Shared;
using NoSqlRepositories.Test.Shared.Entities;

namespace NoSqlRepositories.Tests.MvvX
{
    [TestClass]
    public class SqlCipherCouchbaseRepUnitTest : CouchBaseLiteRepUnitTest
    {
        #region Initialize & Clean

        [ClassInitialize()]
        public static new void ClassInitialize(TestContext testContext)
        {
            CouchBaseLiteRepUnitTest.testContext = testContext;
            NoSQLCoreUnitTests.ClassInitialize(testContext);
        }


        [TestInitialize]
        public override void TestInitialize()
        {
            base.Setup();

            DbName = "nosqltestcblsqlcipherdb";

            // Instanciate the manager only 1 time
            if (CouchBaseLiteLiteManager == null)
            {
                CouchBaseLiteLiteManager = Mvx.Resolve<ICouchBaseLite>();
            }

            // Add Sqlite plugin register. Do it only for unit tests (https://github.com/CouchBaseLite/CouchBaseLite-lite-net/wiki/Error-Dictionary#cblcs0001)
            //CouchBaseLite.Lite.Storage.SystemSQLite.Plugin.Register();
            // Register SQLCipher plugin :
            Couchbase.Lite.Storage.SQLCipher.Plugin.Register();

            entityRepo = GetRepository<TestEntity>(CouchBaseLiteLiteManager, DbName);
            entityRepo2 = GetRepository<TestEntity>(CouchBaseLiteLiteManager, DbName);
            collectionEntityRepo = GetRepository<CollectionTest>(CouchBaseLiteLiteManager, DbName);
            entityExtraEltRepo = GetRepository<TestExtraEltEntity>(CouchBaseLiteLiteManager, DbName);

            // Define mapping for polymorphism
            entityRepo.PolymorphicTypes["TestExtraEltEntity"] = typeof(TestExtraEltEntity);
            entityRepo2.PolymorphicTypes["TestExtraEltEntity"] = typeof(TestExtraEltEntity);

            test = new NoSQLCoreUnitTests(entityRepo,
                                            entityRepo2,
                                            entityExtraEltRepo,
                                            collectionEntityRepo,
                                            NoSQLCoreUnitTests.testContext.DeploymentDirectory,
                                            DbName);
        }

        #endregion

        #region Repository construct

        protected override CouchBaseLiteRepository<T> GetRepository<T>(ICouchBaseLite couchBaseLiteManager, string dbName)
        {
            return new SqlCipherCouchBaseLiteRepository<T>(couchBaseLiteManager, dbName, "passwordTest");
        }

        #endregion
    }
}
