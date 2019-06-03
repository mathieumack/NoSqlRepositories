using Microsoft.VisualStudio.TestTools.UnitTesting;
using NoSqlRepositories.Core.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NoSqlRepositories.Tests.Shared.Helpers;
using NoSqlRepositories.Core;
using NoSqlRepositories.Tests.Shared.Entities;
using NoSqlRepositories.Core.NoSQLException;
using NoSqlRepositories.Tests.Shared.Extensions;
using System.Threading;
using NoSqlRepositories.Core.Queries;

namespace NoSqlRepositories.Tests.Shared
{
    public class NoSQLCoreUnitTests
    {
        #region Members

        protected string baseFilePath;
        protected string dbName;

        protected INoSQLRepository<TestEntity> entityRepo;
        protected INoSQLRepository<TestEntity> entityRepo2;
        protected INoSQLRepository<TestExtraEltEntity> entityExtraEltRepo;

        public static TestContext testContext;

        #endregion

        public NoSQLCoreUnitTests(INoSQLRepository<TestEntity> entityRepo,
                                    INoSQLRepository<TestEntity> entityRepo2,
                                    INoSQLRepository<TestExtraEltEntity> entityExtraEltRepo,
                                    string baseFilePath,
                                    string dbName)
        {
            this.entityRepo = entityRepo;
            this.entityRepo2 = entityRepo2;
            this.entityExtraEltRepo = entityExtraEltRepo;

            this.entityRepo.TruncateCollection();
            this.entityExtraEltRepo.TruncateCollection();

            this.baseFilePath = baseFilePath;
            this.dbName = dbName;
        }

        public static void ClassInitialize(TestContext testContext)
        {
            // Overide datetime.now function
            var now = new DateTime(2016, 01, 01, 0, 0, 0, DateTimeKind.Utc);
            NoSQLRepoHelper.DateTimeUtcNow = (() => now);

            NoSQLCoreUnitTests.testContext = testContext;
        }

        #region Test methods

        public virtual void DatabaseName()
        {
            Assert.IsTrue(entityRepo.DatabaseName.Equals(dbName));
        }

        public virtual void ExpireAt()
        {
            entityRepo.TruncateCollection();

            var entity1 = TestHelper.GetEntity1();
            entityRepo.InsertOne(entity1);
            Assert.IsFalse(string.IsNullOrEmpty(entity1.Id), "DocId has not been set during insert");

            var itemsInDatabase = entityRepo.GetAll();

            // We try to delete the item :
            entityRepo.ExpireAt(entity1.Id, DateTime.Now.AddSeconds(2));

            var itemsInDatabase2 = entityRepo.GetAll();

            Assert.IsTrue(itemsInDatabase.Count() == itemsInDatabase2.Count(), "entityRepo has not been physically deleted after compact");

            Thread.Sleep(4000);

            // We compact the database :
            entityRepo.CompactDatabase();

            var itemsInDatabaseAfterCompact = entityRepo.GetAll();

            Assert.IsTrue(itemsInDatabaseAfterCompact.Count() == itemsInDatabase.Count() - 1, "entityRepo has not been physically deleted after compact");
        }

        public virtual void CompactDatabase()
        {
            entityRepo.TruncateCollection();

            var entity1 = TestHelper.GetEntity1();
            entityRepo.InsertOne(entity1);
            Assert.IsFalse(string.IsNullOrEmpty(entity1.Id), "DocId has not been set during insert");

            var itemsInDatabase = entityRepo.GetAll();
            var itemsInDatabaseCount = itemsInDatabase.Count();

            // We try to delete the item :
            entityRepo.Delete(entity1.Id, false);

            // We compact the database :
            entityRepo.CompactDatabase();

            var itemsInDatabaseAfterCompact = entityRepo.GetAll();
            var itemsInDatabaseAfterCompactCount = itemsInDatabaseAfterCompact.Count();

            Assert.IsTrue(itemsInDatabaseAfterCompactCount == itemsInDatabaseCount - 1, "entityRepo has not been physically deleted after compact");
        }

        public void Count()
        {
            entityRepo.TruncateCollection();
            Assert.AreEqual(0, entityRepo.Count(), "Repo should be empty");

            var entity1 = TestHelper.GetEntity1();
            entityRepo.InsertOne(entity1);
            Assert.AreEqual(1, entityRepo.Count(), "Repo should contain one element");

            var entity2 = TestHelper.GetEntity2();
            entityRepo.InsertOne(entity2);
            Assert.AreEqual(2, entityRepo.Count(), "Repo should contain two elements");

            var entity3 = TestHelper.GetEntity3();
            var entity4 = TestHelper.GetEntity4();
            entityRepo.InsertMany(new List<TestEntity>() { entity3, entity4 });
            Assert.AreEqual(4, entityRepo.Count(), "Repo should contain four elements");
        }

        public void DoQuery()
        {
            entityRepo.TruncateCollection();

            var entity1 = TestHelper.GetEntity1();
            var entity3 = TestHelper.GetEntity3();
            var entity4 = TestHelper.GetEntity4();

            entityRepo.InsertMany(new List<TestEntity>() { entity1, entity3, entity4 });

            // Now we will do some queries :
            var queryOptions = QueryCreator.CreateQueryOptions<TestEntity>(2, 0, null);
            var query = entityRepo.DoQuery(queryOptions);
            Assert.AreEqual(2, query.Count(), "Query should contain three elements");

            // Now we will add a filter method :
            queryOptions = QueryCreator.CreateQueryOptions<TestEntity>(0, 0, (filterItem) =>
            {
                return filterItem.NumberOfChildenInt == 0;
            });
            query = entityRepo.DoQuery(queryOptions);
            Assert.AreEqual(1, query.Count(), "Query should contain two elements");
        }

        public virtual void InsertEntity()
        {
            entityRepo.TruncateCollection();

            var entity1 = TestHelper.GetEntity1();
            entityRepo.InsertOne(entity1);
            Assert.IsFalse(string.IsNullOrEmpty(entity1.Id), "DocId has not been set during insert");

            var t1 = new DateTime(2016, 01, 01, 0, 0, 0, DateTimeKind.Utc);
            var t2 = new DateTime(2017, 01, 01, 0, 0, 0, DateTimeKind.Utc);

            NoSQLRepoHelper.DateTimeUtcNow = (() => t1); // Set the current time to t1

            Exception e = null;
            try
            {
                entityRepo.InsertOne(entity1);
            }
            catch (Exception ex)
            {
                e = ex;
            }

            Assert.IsInstanceOfType(e, typeof(DupplicateKeyNoSQLException), "InsertOne should raise DupplicateKeyException if id already exists");

            var insertResult = entityRepo.InsertOne(entity1, InsertMode.do_nothing_if_key_exists); // Do Nothing
            Assert.AreEqual(InsertResult.not_affected, insertResult, "Expecting not_affected result");

            var entity1_repo = entityRepo.GetById(entity1.Id);
            Assert.IsNotNull(entity1_repo);
            AssertHelper.AreJsonEqual(entity1, entity1_repo);
            Assert.AreEqual(t1, entity1.SystemCreationDate, "SystemCreationDate should be defined during insert");
            Assert.AreEqual(t1, entity1.SystemLastUpdateDate, "SystemLastUpdateDate should be defined during insert");

            // Replace first version
            {
                NoSQLRepoHelper.DateTimeUtcNow = (() => t2); // Set the current time to t2

                var entity1V2 = TestHelper.GetEntity1();
                entity1V2.Id = entity1.Id;

                entity1V2.Name = "Balan2";
                entityRepo.InsertOne(entity1V2, InsertMode.erase_existing); // Erase

                var entity1V2_fromRepo = entityRepo.GetById(entity1.Id);
                Assert.AreEqual(entity1V2_fromRepo.Name, "Balan2", "The insert with erase_existing mode should erase the previous version of the entity");
                Assert.AreEqual(t1, entity1V2.SystemCreationDate, "SystemCreationDate should be the date of the erased entity version");
                Assert.AreEqual(t2, entity1V2.SystemLastUpdateDate, "SystemLastUpdateDate should the date of the update of the entity version");

                AssertHelper.AreJsonEqual(entity1V2, entity1V2_fromRepo);
            }

            // Erase while doc not exists
            {
                //var entity2 = TestHelper.GetEntity2();
                //entityRepo.InsertOne(entity2, InsertMode.erase_existing); // Insert
                //var entity2_repo = entityRepo.GetById(entity2.Id);
                //AssertHelper.AreJsonEqual(entity2, entity2_repo);

                // ABN: why this modification of unit test ?!
                // Clone in order to get a new objet of type TestEntity because a cast is not suffisant
                //var entity2Casted = entity2.CloneToTestEntity();
                //AssertHelper.AreJsonEqual(entity2Casted, entity2_repo);
            }
        }

        public virtual void UpdateEntity()
        {
            entityRepo.TruncateCollection();

            var entity1 = TestHelper.GetEntity1();
            entityRepo.InsertOne(entity1);

            entity1.Name = "NewName";

            var entity1_repo = entityRepo.GetById(entity1.Id);

            Assert.IsNotNull(entity1_repo);
            Assert.AreEqual(entity1_repo.Name, "NewName");
            AssertHelper.AreJsonEqual(entity1, entity1_repo);
        }

        /// <summary>
        /// Validate delete functions of the repository
        /// </summary>
        public virtual void DeleteEntity()
        {
            entityRepo.TruncateCollection();

            var entity1 = TestHelper.GetEntity1();
            entity1.Id = "1";

            var insertResult = entityRepo.InsertOne(entity1);
            Assert.AreEqual(InsertResult.inserted, insertResult, "Expecting inserted result");

            entityRepo.Delete(entity1.Id);

            insertResult = entityRepo.InsertOne(entity1);
            Assert.AreEqual(InsertResult.inserted, insertResult, "Expecting inserted result");
        }

        /// <summary>
        /// Validate delete functions of the repository
        /// </summary>
        public virtual void DeletePhysicalEntity()
        {
            entityRepo.TruncateCollection();

            var entity1 = TestHelper.GetEntity1();
            entity1.Id = "1";

            var insertResult = entityRepo.InsertOne(entity1);
            Assert.AreEqual(InsertResult.inserted, insertResult, "Expecting inserted result");

            entityRepo.Delete(entity1.Id, true);

            insertResult = entityRepo.InsertOne(entity1);
            Assert.AreEqual(InsertResult.inserted, insertResult, "Expecting inserted result");
        }

        public virtual void InsertExtraEltEntity()
        {
            entityExtraEltRepo.TruncateCollection();

            var entity = TestHelper.GetEntity2();
            entityExtraEltRepo.InsertOne(entity);
            Assert.IsFalse(string.IsNullOrEmpty(entity.Id));

            var entity_repo = entityExtraEltRepo.GetById(entity.Id);

            //entity_repo.LookLikeEachOther(entity);

            AssertHelper.AreJsonEqual(entity, entity_repo);
            //Assert.AreEqual<TestEntity>(entity1, entity1_repo);           
        }

        /// <summary>
        /// Currently, each repository is free to returned Datetime in UTC or Local. Client app should handle this case.
        /// </summary>
        public virtual void TimeZoneTest()
        {
            entityRepo.TruncateCollection();
            {
                // Insert local timezone, check if we get same value but in utc format
                var entity1 = TestHelper.GetEntity1();
                entity1.Birthday = new DateTime(1985, 12, 08, 0, 5, 30, DateTimeKind.Local);
                entityRepo.InsertOne(entity1);
                var entity1_repo = entityRepo.GetById(entity1.Id);

                //Assert.AreEqual(DateTimeKind.Utc, entity1_repo.Birthday.Kind, "Returned DB value is not UTC");

                Assert.AreEqual(entity1.Birthday, entity1_repo.Birthday.ToLocalTime(), "Returned DB value is not correct");
            }

            {
                // Insert utc, check if we get local timezone from db
                var entity1 = TestHelper.GetEntity1();
                entity1.Birthday = new DateTime(1985, 12, 08, 0, 0, 0, DateTimeKind.Utc);
                entityRepo.InsertOne(entity1);
                var entity1_repo = entityRepo.GetById(entity1.Id);
                Assert.AreEqual(entity1.Birthday, entity1_repo.Birthday, "Returned DB value is not correct");
            }

        }

        public virtual void GetTests()
        {
            //
            // Test missing element
            //
            entityRepo.TruncateCollection();

            Exception e = null;
            try
            {
                entityRepo.GetById("unknown_id");
            }
            catch (Exception ex)
            {
                e = ex;
            }

            Assert.IsInstanceOfType(e, typeof(KeyNotFoundNoSQLException), "Getbyid sould raise IdNotFoundException for missing ids");
            //
            // Test GetByIds
            //

            // Init test
            var entity1 = TestHelper.GetEntity1();
            var entity2 = TestHelper.GetEntity2();

            entityRepo.InsertOne(entity1);
            entityRepo.InsertOne(entity2);

            List<string> ids = new List<string>() { entity1.Id, entity2.Id, "unknown_id" };
            var results = entityRepo.GetByIds(ids);

            Assert.AreEqual(2, results.Count(), "GetByIds should return 2 entities");
            Assert.IsTrue(results.Any(i => i.Name.Equals("Balan")));
            Assert.IsTrue(results.Any(i => i.Name.Equals("Mack")));
        }

        //public virtual void Polymorphism()
        //{
        //    entityRepo.TruncateCollection();
        //    collectionEntityRepo.TruncateCollection();

        //    TestExtraEltEntity entity2 = TestHelper.GetEntity2();
        //    entityRepo.InsertOne(entity2);
        //    Assert.IsFalse(string.IsNullOrEmpty(entity2.Id));

        //    var entity2_repo = entityRepo.GetById(entity2.Id);

        //    //entity_repo.LookLikeEachOther(entity);

        //    AssertHelper.AreJsonEqual(entity2, entity2_repo, ErrorMsg: "Get of a TestExtraEltEntity instance from a TestEntity repo should return TestExtraEltEntity");
        //    //Assert.AreEqual<TestEntity>(entity1, entity1_repo);

        //    var collectionTest = new CollectionTest();
        //    collectionTest.PolymorphCollection.Add(entity2); // TestExtraEltEntity instance
        //    collectionTest.PolymorphCollection.Add(TestHelper.GetEntity1()); // TestEntity instance

        //    collectionEntityRepo.InsertOne(collectionTest);
        //    var collectionTest_fromRepo = collectionEntityRepo.GetById(collectionTest.Id);

        //    AssertHelper.AreJsonEqual(collectionTest, collectionTest_fromRepo, ErrorMsg: "Check if collection elements has the good type");
        //}

        private string getFullpath(string filepath)
        {
            return Path.Combine(Directory.GetCurrentDirectory(), filepath);
        }

        public virtual void Attachments()
        {
            // Prepare test
            testContext.DeployFile(@"Ressources\Images\TN_15.jpg", @"Images");
            testContext.DeployFile(@"Ressources\Images\RoNEX_brochure.pdf", @"Images");
            entityRepo.TruncateCollection();

            string attach1FilePath = "Images/TN_15.jpg";
            string attach1FileName = "IDFile_1";

            string attach2FilePath = "Images/RoNEX_brochure.pdf";
            string attach2FileName = "IDFile_2";

            TestEntity entity1;

            //
            // Test add of attachements on a First entity
            //
            {
                entity1 = TestHelper.GetEntity1();
                entityRepo.InsertOne(entity1, InsertMode.erase_existing);
                Assert.IsFalse(string.IsNullOrEmpty(entity1.Id), "Id has been defined during insert");

                using (var fileStream = File.Open(getFullpath(attach1FilePath), FileMode.Open))
                {
                    entityRepo.AddAttachment(entity1.Id, fileStream, "image/jpg", attach1FileName);
                }

                using (var fileStream = File.Open(getFullpath(attach2FilePath), FileMode.Open))
                {
                    entityRepo.AddAttachment(entity1.Id, fileStream, "application/pdf", attach2FileName);
                }

                // Try to get the list of attachments
                var attachNames = entityRepo.GetAttachmentNames(entity1.Id);

                Assert.AreEqual(2, attachNames.Count(), "Invalid number of attachments names found");
                Assert.IsTrue(attachNames.Contains(attach1FileName), "First attachment not found in the list");
                Assert.IsTrue(attachNames.Contains(attach2FileName), "2nd attachment not found in the list");

                entity1.Name = "NewName";
                entityRepo.Update(entity1);
                var attachNames2 = entityRepo.GetAttachmentNames(entity1.Id);
                Assert.AreEqual(2, attachNames.Count(), "An update of an entity should not alter its attachments");
                Assert.IsTrue(attachNames.Contains(attach1FileName), "An update of an entity should not alter its attachments");
                Assert.IsTrue(attachNames.Contains(attach2FileName), "An update of an entity should not alter its attachments");


            }

            //
            // Test add of the same file to a 2nd entity
            //
            {
                //var entity2 = TestHelper.GetEntity2();
                //entityRepo.InsertOne(entity2, InsertMode.erase_existing);
                //Assert.IsFalse(string.IsNullOrEmpty(entity2.Id), "Id has been defined during insert");

                //using (var fileStream = File.Open(getFullpath(attach1FilePath), FileMode.Open))
                //{
                //    entityRepo.AddAttachment(entity2.Id, fileStream, "image/jpg", attach1FileName);
                //}
            }

            //
            // Test get an attachement
            //
            {
                using (var fileRepoStream = entityRepo.GetAttachment(entity1.Id, attach1FileName))
                {
                    Assert.IsNotNull(fileRepoStream, "The steam returned by GetAttachment should not be null");

                    using (var sourceFileSteam = File.Open(getFullpath(attach1FilePath), FileMode.Open))
                    {
                        Assert.IsTrue(sourceFileSteam.Length > 0, "File content is empty");
                    }
                }
            }

            //
            // Test remove of an attachement
            //
            {
                entityRepo.RemoveAttachment(entity1.Id, attach1FileName);

                AttachmentNotFoundNoSQLException notfoundEx = null;
                try
                {
                    var fileRepoStream = entityRepo.GetAttachment(entity1.Id, attach1FileName);
                }
                catch (AttachmentNotFoundNoSQLException ex)
                {
                    notfoundEx = ex;
                }

                Assert.IsInstanceOfType(notfoundEx, typeof(AttachmentNotFoundNoSQLException), "The get should return exception because the attachement has been deleted");

                var attachNames3 = entityRepo.GetAttachmentNames(entity1.Id);
                Assert.AreEqual(1, attachNames3.Count());

                entityRepo.Delete(entity1.Id);
                entityRepo.InsertOne(entity1);

                var attachNames4 = entityRepo.GetAttachmentNames(entity1.Id);
                Assert.AreEqual(0, attachNames4.Count(), "Delete of an entity should delete its attachemnts");
            }

            //
            // Test remove of a missing attachement
            //
            {
                AttachmentNotFoundNoSQLException notfoundEx = null;
                try
                {
                    entityRepo.RemoveAttachment(entity1.Id, attach1FileName);
                }
                catch (AttachmentNotFoundNoSQLException ex)
                {
                    notfoundEx = ex;
                }

                Assert.IsInstanceOfType(notfoundEx, typeof(AttachmentNotFoundNoSQLException), "The RemoveAttachment should return exception because the attachement doesn't exists");
            }
        }

        /// <summary>
        /// Init Set demo
        /// => Copy files in : C:\Users\abala\AppData\Roaming
        /// </summary>
        public virtual void Close()
        {
            entityRepo.TruncateCollection();

            var entity1 = TestHelper.GetEntity1();
            entityRepo.InsertOne(entity1);

            var entitylist = entityRepo.GetAll();
            Assert.AreEqual(1, entitylist.Count(), "Invalide number. The expected result is " + entitylist.Count());

            var task = Task.Run(() => entityRepo.Close());
            task.Wait();

            Assert.IsFalse(entityRepo.ConnectionOpened, "The connection must be closed");
            try
            {
                entityRepo.GetAll();
                Assert.Fail();
            }
            catch (InvalidOperationException)
            {
                // Good !
            }
            catch (Exception)
            {
                Assert.Fail();
            }
            // Now we try to get datas
        }

        /// <summary>
        /// Init Set demo
        /// => Copy files in : C:\Users\abala\AppData\Roaming
        /// </summary>
        public virtual void ConnectAgain()
        {
            entityRepo.TruncateCollection();

            var entity1 = TestHelper.GetEntity1();
            entityRepo.InsertOne(entity1);

            var entitylist = entityRepo.GetAll();
            Assert.AreEqual(1, entitylist.Count(), "Invalide number. The expected result is " + entitylist.Count());

            var task = Task.Run(() => entityRepo.Close());
            task.Wait();

            Assert.IsFalse(entityRepo.ConnectionOpened, "The connection must be closed");
            try
            {
                entityRepo.GetAll();
                Assert.Fail();
            }
            catch (InvalidOperationException)
            {
                // Good !
            }
            catch (Exception)
            {
                Assert.Fail();
            }
            // Now we try to connect again
            entityRepo.ConnectAgain();

            entitylist = entityRepo.GetAll();
            Assert.AreEqual(1, entitylist.Count(), "Invalide number. The expected result is " + entitylist.Count());
        }

        /// <summary>
        /// Init Set demo
        /// => Copy files in : C:\Users\abala\AppData\Roaming
        /// </summary>
        public virtual void GetAll()
        {
            entityRepo.TruncateCollection();

            var entity1 = TestHelper.GetEntity1();
            entityRepo.InsertOne(entity1);

            var entity2 = TestHelper.GetEntity2();
            entityRepo.InsertOne(entity2);

            var entity3 = TestHelper.GetEntity3();
            entityRepo.InsertOne(entity3);

            var entity4 = TestHelper.GetEntity4();
            entityRepo.InsertOne(entity4);

            entityRepo.Delete(entity3.Id);

            var entitylist = entityRepo.GetAll();
            Assert.AreEqual(3, entitylist.Count(), "Invalid count items.");

            foreach (var e in entitylist)
            {
                Assert.IsNotNull(e, "Entity returned should not be null");
            }

            //var collectionTest = new CollectionTest();
            //collectionTest.PolymorphCollection.Add(entity1); // TestExtraEltEntity instance

            //var collectionTest2 = new CollectionTest();
            //collectionTest2.PolymorphCollection.Add(entity2); // TestExtraEltEntity instance

            //collectionEntityRepo.InsertOne(collectionTest);
            //collectionEntityRepo.InsertOne(collectionTest2);

            //var entityCollectionlist = collectionEntityRepo.GetAll();
            //Assert.AreEqual(2, entityCollectionlist.Count(), "Bad number of doc. We should not return entities of an other collection");

            //var entitylist2 = entityRepo.GetAll();
            //Assert.AreEqual(3, entitylist2.Count(), "Bad number of doc. We should not return entities of an other collection");

            //collectionEntityRepo.TruncateCollection();
            //entitylist2 = entityRepo.GetAll();
            //Assert.AreEqual(3, entitylist2.Count(), "Truncate of a collection should not affect other collections");

        }

        public virtual void ConcurrentAccess(bool parallel)
        {
            var repo1 = this.entityRepo;
            var repo2 = this.entityRepo2;

            repo1.TruncateCollection();

            //
            // Insert 3 set of entities
            //

            int nbDoc = 500;

            var t1 = Task.Run(
                () => InsertEntities(nbDoc, 0, repo1));

            if (!parallel)
                t1.Wait();

            var t2 = Task.Run(
                () => InsertEntities(nbDoc, nbDoc, repo2));

            if (!parallel)
                t2.Wait();

            var t3 = Task.Run(
                () => InsertEntities(nbDoc, nbDoc * 2, repo2));

            if (!parallel)
                t3.Wait();

            try
            {
                if (parallel)
                    Task.WaitAll(t1, t2, t3);
            }
            catch (AggregateException ex)
            {
                // Unwrap AggregateException
                throw ex.InnerException;
            }

            // Get from Repo 2 an entity Inserted in Repo 1
            try
            {
                var getEntity1Res = repo2.GetById("1");
            }
            catch (KeyNotFoundNoSQLException)
            {
                Assert.Fail("Should not raise KeyNotFoundNoSQLException");
            }

            // Delete from Repo 2 an entity Inserted in Repo 1
            repo2.Delete("1");

            // Get from Repo 1 an entity Deleted in Repo 1
            Exception exRes = null;
            try
            {
                var getEntity1Res = repo1.GetById("1");
                Assert.Fail("Repo1 should raise KeyNotFoundNoSQLException");
            }
            catch (Exception ex)
            {
                exRes = ex;
            }


            // Get from Repo 2 an entity Inserted in Repo 1
            var entity2FromRepo1 = repo1.GetById("2");
            var entity2FromRepo2 = repo2.GetById("2");

            entity2FromRepo1.Name = "NameUpdatedInRepo1";
            repo1.Update(entity2FromRepo1);

            Assert.AreNotEqual("NameUpdatedInRepo1", entity2FromRepo2, "Object instance from Repo 2 should not be affected");

            var entity2FromRepo2AfterUpdate = repo2.GetById("2");
            Assert.AreEqual("NameUpdatedInRepo1", entity2FromRepo2AfterUpdate.Name, "Object instance from Repo 2 should have been updated with Repo 1 modification");

        }

        public virtual void ViewTests()
        {
            entityRepo.TruncateCollection();

            //
            // Add test data
            //
            var entity1 = TestHelper.GetEntity1();
            entity1.Id = "1";
            entityRepo.InsertOne(entity1);

            var entity2 = TestHelper.GetEntity2();
            entity2.Id = "2";
            entityRepo.InsertOne(entity2);

            // Add the 3td et 4th entities to en secondary repo to ensure insert are visible throw all repositories
            var entity3 = TestHelper.GetEntity3();
            entity3.Id = "3";
            entityRepo.InsertOne(entity3);

            var entity4 = TestHelper.GetEntity4();
            entity4.Id = "4";
            entityRepo.InsertOne(entity4);

            //
            // Get data from an "Int" field
            //

            // Try on an other repo
            //var res12 = entityRepo2.GetByField<int>(nameof(TestEntity.NumberOfChildenInt), 0).OrderBy(e => e.Id).ToList();

            //// Filter on 1 value
            //var res1 = entityRepo.GetByField<int>(nameof(TestEntity.NumberOfChildenInt), 0).OrderBy(e => e.Id).ToList();


            //Assert.AreEqual(2, res1.Count);
            //Assert.AreEqual("2", res1[0].Id);
            //Assert.AreEqual("3", res1[1].Id);
            //AssertHelper.AreJsonEqual(entity2, res1[0]);
            //AssertHelper.AreJsonEqual(entity3, res1[1]);

            //var res2 = entityRepo.GetByField<int>(nameof(TestEntity.NumberOfChildenInt), 0).OrderBy(e => e.Id).ToList();
            //Assert.AreEqual(2, res2.Count, "Check the an error not occured after a 2 call (the object entities are in memory)");

            //List<string> res3 = entityRepo.GetKeyByField<int>(nameof(TestEntity.NumberOfChildenInt), 0).OrderBy(e => e).ToList();
            //Assert.AreEqual("2", res3[0]);
            //Assert.AreEqual("3", res3[1]);

            //Exception expectedEx = null;
            //try
            //{
            //    entityRepo.GetByField<int>(nameof(TestEntity.NumberOfChildenLong), 0).OrderBy(e => e.Id).ToList();
            //}
            //catch (Exception ex)
            //{

            //    expectedEx = ex;
            //}
            //Assert.IsInstanceOfType(expectedEx, typeof(IndexNotFoundNoSQLException));

            //// Filter on a set of value

            //var searchedValues = new List<int> { 0, 10 };
            //var res4 = entityRepo.GetByField<int>(nameof(TestEntity.NumberOfChildenInt), searchedValues).OrderBy(e => e.Id).ToList();
            //Assert.AreEqual(3, res4.Count);

            //List<string> res5 = entityRepo.GetKeyByField<int>(nameof(TestEntity.NumberOfChildenInt), searchedValues).OrderBy(e => e).ToList();
            //Assert.AreEqual(3, res5.Count);

            ////
            //// Get data from a "List<string>" field
            ////
            //var res6 = entityRepo.GetByField<string>(nameof(TestEntity.Cities), "Grenoble").OrderBy(e => e.Id).ToList();
            //Assert.AreEqual(3, res6.Where(e => e.Cities.Contains("Grenoble")).Count());

            //var searchList = new List<string> { "Grenoble", "Andernos" };
            //var res7 = entityRepo.GetByField<string>(nameof(TestEntity.Cities), searchList).OrderBy(e => e.Id).ToList();
            //Assert.AreEqual(3, res7.Count, "Dupplicate entries should be removed");
            //Assert.AreEqual(3, res7.Where(e => e.Cities.Contains("Grenoble")).Count());
            //Assert.AreEqual(1, res7.Where(e => e.Cities.Contains("Andernos")).Count());

            //var res8 = entityRepo.GetByField<string>(nameof(TestEntity.Cities), "Grenoble").OrderBy(e => e.Id).ToList();
            //Assert.AreEqual(3, res8.Count);

            //List<string> res9 = entityRepo.GetKeyByField<string>(nameof(TestEntity.Cities), searchList).OrderBy(e => e).ToList();
            //Assert.AreEqual(3, res9.Count, "Dupplicate entries should be removed");

            //var res10 = entityRepo.GetByField<string>(nameof(TestEntity.Cities), "GrEnObLe").OrderBy(e => e.Id).ToList();
            //Assert.AreEqual(0, res10.Count, "String comparison should be case sensitive");

            //var res11 = entityRepo.GetByField<string>(nameof(TestEntity.Cities), "Grenoblé").OrderBy(e => e.Id).ToList();
            //Assert.AreEqual(0, res11.Count, "String comparison should be accent sensitive");

        }

        #endregion

        #region Private

        // Merged From linked CopyStream below and Jon Skeet's ReadFully example
        private bool CompareStreams(Stream input1, Stream input2)
        {
            byte[] buffer1 = new byte[16 * 1024];
            byte[] buffer2 = new byte[16 * 1024];

            int read;
            while ((read = input1.Read(buffer1, 0, buffer1.Length)) > 0)
            {
                input2.Read(buffer2, 0, buffer2.Length);

                if (!buffer1.SequenceEqual(buffer2))
                    return false;
            }

            return true;
        }

        private void InsertEntities(int nbEntities, int firstId, INoSQLRepository<TestEntity> repo)
        {
            for (int i = 1; i <= nbEntities; i++)
            {
                Console.WriteLine(i);
                TestEntity e = new TestEntity
                {
                    Id = (firstId + i).ToString(),
                    PoidsDouble = Faker.RandomNumber.Next(),
                    NumberOfChildenInt = Faker.RandomNumber.Next(),
                    Name = Faker.Name.FullName()
                };

                repo.InsertOne(e);
            }
        }

        #endregion
    }
}
