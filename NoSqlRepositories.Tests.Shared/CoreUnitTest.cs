//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using NoSqlRepositories.Core;
//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace NoSqlRepositories.Tests.Shared
//{
//    public abstract class CoreUnitTest
//    {
//        #region Members

//        protected string baseFilePath;
//        protected string dbName;

//        protected INoSQLRepository<TestEntity> entityRepo;
//        protected INoSQLRepository<TestEntity> entityRepo2;
//        protected INoSQLRepository<TestExtraEltEntity> entityExtraEltRepo;

//        public static TestContext testContext;

//        #endregion

//        public NoSQLCoreUnitTests(INoSQLRepository<TestEntity> entityRepo,
//                                    INoSQLRepository<TestEntity> entityRepo2,
//                                    INoSQLRepository<TestExtraEltEntity> entityExtraEltRepo,
//                                    string baseFilePath,
//                                    string dbName)
//        {
//            this.entityRepo = entityRepo;
//            this.entityRepo2 = entityRepo2;
//            this.entityExtraEltRepo = entityExtraEltRepo;

//            this.entityRepo.TruncateCollection();
//            this.entityExtraEltRepo.TruncateCollection();

//            this.baseFilePath = baseFilePath;
//            this.dbName = dbName;
//        }
//    }
//}
