//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using NoSqlRepositories.Core.Helpers;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Threading.Tasks;
//using NoSqlRepositories.Test.Shared.Helpers;
//using NoSqlRepositories.Core;
//using NoSqlRepositories.Test.Shared.Entities;
//using NoSqlRepositories.Core.NoSQLException;
//using NoSqlRepositories.Test.Shared.Extensions;

//namespace NoSqlRepositories.Test.Shared
//{
//    public class AsyncNoSQLCoreUnitTests
//    {
//        #region Members

//        protected string baseFilePath;

//        protected IAsyncNoSQLRepository<TestEntity> entityRepo;
//        protected IAsyncNoSQLRepository<TestEntity> entityRepo2;
//        protected IAsyncNoSQLRepository<TestExtraEltEntity> entityExtraEltRepo;
//        protected IAsyncNoSQLRepository<CollectionTest> collectionEntityRepo;

//        public static TestContext testContext;

//        #endregion

//        public AsyncNoSQLCoreUnitTests(IAsyncNoSQLRepository<TestEntity> entityRepo,
//                                        IAsyncNoSQLRepository<TestEntity> entityRepo2,
//                                        IAsyncNoSQLRepository<TestExtraEltEntity> entityExtraEltRepo,
//                                        IAsyncNoSQLRepository<CollectionTest> collectionEntityRepo,
//            string baseFilePath)
//        {
//            this.entityRepo = entityRepo;
//            this.entityRepo2 = entityRepo2;
//            this.entityExtraEltRepo = entityExtraEltRepo;
//            this.collectionEntityRepo = collectionEntityRepo;

//            this.entityRepo.TruncateCollection();
//            this.entityExtraEltRepo.TruncateCollection();
//            this.collectionEntityRepo.TruncateCollection();

//            this.baseFilePath = baseFilePath;
//        }

//        public static void ClassInitialize(TestContext testContext)
//        {
//            // Overide datetime.now function
//            var now = new DateTime(2016, 01, 01, 0, 0, 0, DateTimeKind.Utc);
//            NoSQLRepoHelper.DateTimeUtcNow = (() => now);

//            NoSQLCoreUnitTests.testContext = testContext;
//        }

//        #region Test methods

//        public virtual async Task InsertEntity()
//        {
//            await entityRepo.TruncateCollection();

//            var entity1 = TestHelper.GetEntity1();
//            await entityRepo.InsertOne(entity1);
//            Assert.IsFalse(string.IsNullOrEmpty(entity1.Id), "DocId has not been set during insert");

//            var t1 = new DateTime(2016, 01, 01, 0, 0, 0, DateTimeKind.Utc);
//            var t2 = new DateTime(2017, 01, 01, 0, 0, 0, DateTimeKind.Utc);

//            NoSQLRepoHelper.DateTimeUtcNow = (() => t1); // Set the current time to t1

//            Exception e = null;
//            try
//            {
//                await entityRepo.InsertOne(entity1);
//            }
//            catch (Exception ex)
//            {
//                e = ex;
//            }

//            Assert.IsInstanceOfType(e, typeof(DupplicateKeyNoSQLException), "InsertOne should raise DupplicateKeyException if id already exists");

//            var insertResult = await entityRepo.InsertOne(entity1, InsertMode.do_nothing_if_key_exists); // Do Nothing
//            Assert.AreEqual(InsertResult.not_affected, insertResult, "Expecting not_affected result");
            
//            var entity1_repo = await entityRepo.GetById(entity1.Id);
//            Assert.IsNotNull(entity1_repo);
//            AssertHelper.AreJsonEqual(entity1, entity1_repo);
//            Assert.AreEqual(t1, entity1.SystemCreationDate, "SystemCreationDate should be defined during insert");
//            Assert.AreEqual(t1, entity1.SystemLastUpdateDate, "SystemLastUpdateDate should be defined during insert");

//            // Replace first version
//            {
//                NoSQLRepoHelper.DateTimeUtcNow = (() => t2); // Set the current time to t2

//                var entity1V2 = TestHelper.GetEntity1();
//                entity1V2.Id = entity1.Id;

//                entity1V2.Name = "Balan2";
//                await entityRepo.InsertOne(entity1V2, InsertMode.erase_existing); // Erase

//                var entity1V2_fromRepo = await entityRepo.GetById(entity1.Id);
//                Assert.AreEqual(entity1V2_fromRepo.Name, "Balan2", "The insert with erase_existing mode should erase the previous version of the entity");
//                Assert.AreEqual(t1, entity1V2.SystemCreationDate, "SystemCreationDate should be the date of the erased entity version");
//                Assert.AreEqual(t2, entity1V2.SystemLastUpdateDate, "SystemLastUpdateDate should the date of the update of the entity version");

//                AssertHelper.AreJsonEqual(entity1V2, entity1V2_fromRepo);
//            }

//            // Erase while doc not exists
//            {
//                var entity2 = TestHelper.GetEntity2();
//                await entityRepo.InsertOne(entity2, InsertMode.erase_existing); // Insert
//                var entity2_repo = await entityRepo.GetById(entity2.Id);
//                AssertHelper.AreJsonEqual(entity2, entity2_repo);
                
//                // ABN: why this modification of unit test ?!
//                // Clone in order to get a new objet of type TestEntity because a cast is not suffisant
//                //var entity2Casted = entity2.CloneToTestEntity();
//                //AssertHelper.AreJsonEqual(entity2Casted, entity2_repo);
//            }
//        }

//        public virtual async Task UpdateEntity()
//        {
//            await entityRepo.TruncateCollection();

//            var entity1 = TestHelper.GetEntity1();
//            await entityRepo.InsertOne(entity1);

//            entity1.Name = "NewName";

//            var entity1_repo = await entityRepo.GetById(entity1.Id);

//            Assert.IsNotNull(entity1_repo);
//            Assert.AreEqual(entity1_repo.Name, "NewName");
//            AssertHelper.AreJsonEqual(entity1, entity1_repo);            
//        }

//        /// <summary>
//        /// Validate delete functions of the repository
//        /// </summary>
//        public virtual async Task DeleteEntity()
//        {
//            await entityRepo.TruncateCollection();

//            var entity1 = TestHelper.GetEntity1();
//            entity1.Id = "1";

//            var insertResult = await entityRepo.InsertOne(entity1);
//            Assert.AreEqual(InsertResult.inserted, insertResult, "Expecting inserted result");

//            await entityRepo.Delete(entity1.Id);

//            insertResult = await entityRepo.InsertOne(entity1);
//            Assert.AreEqual(InsertResult.inserted, insertResult, "Expecting inserted result");
//        }

//        /// <summary>
//        /// Validate delete functions of the repository
//        /// </summary>
//        public virtual async Task DeletePhysicalEntity()
//        {
//            await entityRepo.TruncateCollection();

//            var entity1 = TestHelper.GetEntity1();
//            entity1.Id = "1";

//            var insertResult = await entityRepo.InsertOne(entity1);
//            Assert.AreEqual(InsertResult.inserted, insertResult, "Expecting inserted result");

//            await entityRepo.Delete(entity1.Id, true);

//            insertResult = await entityRepo.InsertOne(entity1);
//            Assert.AreEqual(InsertResult.inserted, insertResult, "Expecting inserted result");
//        }

//        public virtual async Task InsertExtraEltEntity()
//        {
//            await entityExtraEltRepo.TruncateCollection();

//            var entity = TestHelper.GetEntity2();
//            await entityExtraEltRepo.InsertOne(entity);
//            Assert.IsFalse(string.IsNullOrEmpty(entity.Id));

//            var entity_repo = await entityExtraEltRepo.GetById(entity.Id);

//            //entity_repo.LookLikeEachOther(entity);

//            AssertHelper.AreJsonEqual(entity, entity_repo);
//            //Assert.AreEqual<TestEntity>(entity1, entity1_repo);           
//        }

//        /// <summary>
//        /// Currently, each repository is free to returned Datetime in UTC or Local. Client app should handle this case.
//        /// </summary>
//        public virtual async Task TimeZoneTest()
//        {
//            await entityRepo.TruncateCollection();

//            // Insert local timezone, check if we get same value but in utc format
//            var entity1 = TestHelper.GetEntity1();
//            entity1.Birthday = new DateTime(1985, 12, 08, 0, 5, 30, DateTimeKind.Local);
//            await entityRepo.InsertOne(entity1);
//            var entity1_repo = await entityRepo.GetById(entity1.Id);

//            //Assert.AreEqual(DateTimeKind.Utc, entity1_repo.Birthday.Kind, "Returned DB value is not UTC");

//            Assert.AreEqual(entity1.Birthday, entity1_repo.Birthday.ToLocalTime(), "Returned DB value is not correct");

//            // Insert utc, check if we get local timezone from db
//            entity1 = TestHelper.GetEntity1();
//            entity1.Birthday = new DateTime(1985, 12, 08, 0, 0, 0, DateTimeKind.Utc);
//            await entityRepo.InsertOne(entity1);
//            entity1_repo = await entityRepo.GetById(entity1.Id);
//            Assert.AreEqual(entity1.Birthday, entity1_repo.Birthday, "Returned DB value is not correct");
//        }

//        public virtual async Task GetTests()
//        {
//            await entityRepo.TruncateCollection();

//            Exception e = null;
//            try
//            {
//                await entityRepo.GetById("unknown_id");
//            }
//            catch (Exception ex)
//            {
//                e = ex;
//            }

//            Assert.IsInstanceOfType(e, typeof(KeyNotFoundNoSQLException), "Getbyid sould raise IdNotFoundException for missing ids");           
//        }

//        public virtual async Task Polymorphism()
//        {
//            await entityRepo.TruncateCollection();
//            await collectionEntityRepo.TruncateCollection();

//            TestExtraEltEntity entity2 = TestHelper.GetEntity2();
//            await entityRepo.InsertOne(entity2);
//            Assert.IsFalse(string.IsNullOrEmpty(entity2.Id));

//            var entity2_repo = await entityRepo.GetById(entity2.Id);
            
//            //entity_repo.LookLikeEachOther(entity);

//            AssertHelper.AreJsonEqual(entity2, entity2_repo, ErrorMsg: "Get of a TestExtraEltEntity instance from a TestEntity repo should return TestExtraEltEntity");
//            //Assert.AreEqual<TestEntity>(entity1, entity1_repo);

//            var collectionTest = new CollectionTest();
//            collectionTest.PolymorphCollection.Add(entity2); // TestExtraEltEntity instance
//            collectionTest.PolymorphCollection.Add(TestHelper.GetEntity1()); // TestEntity instance

//            await collectionEntityRepo.InsertOne(collectionTest);
//            var collectionTest_fromRepo = await collectionEntityRepo.GetById(collectionTest.Id);

//            AssertHelper.AreJsonEqual(collectionTest, collectionTest_fromRepo, ErrorMsg: "Check if collection elements has the good type");            
//        }

//        private string getFullpath(string filepath)
//        {
//            return Path.Combine(this.baseFilePath, filepath);
//        }

//        public virtual async Task Attachments()
//        {
//            // Prepare test
//            testContext.DeployFile(@"Ressources\Images\TN_15.jpg", @"Images");
//            testContext.DeployFile(@"Ressources\Images\RoNEX_brochure.pdf", @"Images");
//            await entityRepo.TruncateCollection();

//            string attach1FilePath = "Images/TN_15.jpg";
//            string attach1FileName = "IDFile_1";

//            string attach2FilePath = "Images/RoNEX_brochure.pdf";
//            string attach2FileName = "IDFile_2";

//            TestEntity entity1;

//            //
//            // Test add of attachements on a First entity
//            //
//            entity1 = TestHelper.GetEntity1();
//            await entityRepo.InsertOne(entity1, InsertMode.erase_existing);
//            Assert.IsFalse(string.IsNullOrEmpty(entity1.Id), "Id has been defined during insert");

//            using (var fileStream = File.Open(getFullpath(attach1FilePath), FileMode.Open))
//            {
//                await entityRepo.AddAttachment(entity1.Id, fileStream, "image/jpg", attach1FileName);
//            }

//            using (var fileStream = File.Open(getFullpath(attach2FilePath), FileMode.Open))
//            {
//                await entityRepo.AddAttachment(entity1.Id, fileStream, "application/pdf", attach2FileName);
//            }

//            // Try to get the list of attachments
//            var attachNames = await entityRepo.GetAttachmentNames(entity1.Id);

//            Assert.AreEqual(2, attachNames.Count, "Invalid number of attachments names found");
//            Assert.IsTrue(attachNames.Contains(attach1FileName), "First attachment not found in the list");
//            Assert.IsTrue(attachNames.Contains(attach2FileName), "2nd attachment not found in the list");

//            entity1.Name = "NewName";
//            await entityRepo.Update(entity1);
//            var attachNames2 = entityRepo.GetAttachmentNames(entity1.Id);
//            Assert.AreEqual(2, attachNames.Count, "An update of an entity should not alter its attachments");
//            Assert.IsTrue(attachNames.Contains(attach1FileName), "An update of an entity should not alter its attachments");
//            Assert.IsTrue(attachNames.Contains(attach2FileName), "An update of an entity should not alter its attachments");


//            //
//            // Test add of the same file to a 2nd entity
//            //
//            var entity2 = TestHelper.GetEntity2();
//            await entityRepo.InsertOne(entity2, InsertMode.erase_existing);
//            Assert.IsFalse(string.IsNullOrEmpty(entity2.Id), "Id has been defined during insert");

//            using (var fileStream = File.Open(getFullpath(attach1FilePath), FileMode.Open))
//            {
//                await entityRepo.AddAttachment(entity2.Id, fileStream, "image/jpg", attach1FileName);
//            }

//            //
//            // Test get an attachement
//            //
//            using (var fileRepoStream = await entityRepo.GetAttachment(entity1.Id, attach1FileName))
//            {
//                Assert.IsNotNull(fileRepoStream, "The steam returned by GetAttachment should not be null");

//                using (var sourceFileSteam = File.Open(getFullpath(attach1FilePath), FileMode.Open))
//                {
//                    Assert.IsTrue(CompareStreams(sourceFileSteam, fileRepoStream), "File content is different");
//                }
//            }

//            //
//            // Test remove of an attachement
//            //
//            await entityRepo.RemoveAttachment(entity1.Id, attach1FileName);

//            AttachmentNotFoundNoSQLException notfoundEx = null;
//            try
//            {
//                var fileRepoStream = await entityRepo.GetAttachment(entity1.Id, attach1FileName);
//            }
//            catch (AttachmentNotFoundNoSQLException ex)
//            {
//                notfoundEx = ex;
//            }

//            Assert.IsInstanceOfType(notfoundEx, typeof(AttachmentNotFoundNoSQLException), "The get should return exception because the attachement has been deleted");

//            var attachNames3 = await entityRepo.GetAttachmentNames(entity1.Id);
//            Assert.AreEqual(1, attachNames3.Count);

//            await entityRepo.Delete(entity1.Id);
//            await entityRepo.InsertOne(entity1);

//            var attachNames4 = await entityRepo.GetAttachmentNames(entity1.Id);
//            Assert.AreEqual(0, attachNames4.Count, "Delete of an entity should delete its attachemnts");

//            //
//            // Test remove of a missing attachement
//            //
//            notfoundEx = null;
//            try
//            {
//                await entityRepo.RemoveAttachment(entity1.Id, attach1FileName);
//            }
//            catch (AttachmentNotFoundNoSQLException ex)
//            {
//                notfoundEx = ex;
//            }

//            Assert.IsInstanceOfType(notfoundEx, typeof(AttachmentNotFoundNoSQLException), "The RemoveAttachment should return exception because the attachement doesn't exists");
//        }

  
//        /// <summary>
//        /// Init Set demo
//        /// => Copy files in : C:\Users\abala\AppData\Roaming
//        /// </summary>
//        public virtual async Task GetAll()
//        {
//            await entityRepo.TruncateCollection();

//            var entity1 = TestHelper.GetEntity1();
//            await entityRepo.InsertOne(entity1);

//            var entity2 = TestHelper.GetEntity2();
//            await entityRepo.InsertOne(entity2);

//            var entity3 = TestHelper.GetEntity3();
//            await entityRepo.InsertOne(entity3);

//            var entity4 = TestHelper.GetEntity4();
//            await entityRepo.InsertOne(entity4);

//            await entityRepo.Delete(entity3.Id);

//            var entitylist = await entityRepo.GetAll();
//            Assert.AreEqual(3, entitylist.Count, "Invalide number. The expected result is " + entitylist.Count);

//            foreach (var e in entitylist)
//            {
//                Assert.IsNotNull(e, "Entity returned should not be null");
//            }

//            var collectionTest = new CollectionTest();
//            collectionTest.PolymorphCollection.Add(entity1); // TestExtraEltEntity instance

//            var collectionTest2 = new CollectionTest();
//            collectionTest2.PolymorphCollection.Add(entity2); // TestExtraEltEntity instance

//            await collectionEntityRepo.InsertOne(collectionTest);
//            await collectionEntityRepo.InsertOne(collectionTest2);

//            var entityCollectionlist = await collectionEntityRepo.GetAll();
//            Assert.AreEqual(2, entityCollectionlist.Count, "Bad number of doc. We should not return entities of an other collection");

//            var entitylist2 = await entityRepo.GetAll();
//            Assert.AreEqual(3, entitylist2.Count, "Bad number of doc. We should not return entities of an other collection");

//            await collectionEntityRepo.TruncateCollection();
//            entitylist2 = await entityRepo.GetAll();
//            Assert.AreEqual(3, entitylist2.Count, "Truncate of a collection should not affect other collections");

//        }

//        public virtual async Task ConcurrentAccess(bool parallel)
//        {
//            var repo1 = this.entityRepo;
//            var repo2 = this.entityRepo2;

//            await repo1.TruncateCollection();

//            //
//            // Insert 3 set of entities
//            //

//            int nbDoc = 500;

//            var t1 = Task.Run(
//                () => InsertEntities(nbDoc, 0, repo1));

//            if (!parallel)
//                t1.Wait();

//            var t2 = Task.Run(
//                () => InsertEntities(nbDoc, nbDoc, repo2));
            
//            if (!parallel)
//                t2.Wait();

//            var t3 = Task.Run(
//                () => InsertEntities(nbDoc, nbDoc*2, repo2));

//            if (!parallel)
//                t3.Wait();

//            try
//            {
//                if (parallel)
//                    Task.WaitAll(t1, t2, t3);
//            }
//            catch (AggregateException ex)
//            {
//                // Unwrap AggregateException
//                throw ex.InnerException;
//            }

//            // Get from Repo 2 an entity Inserted in Repo 1
//            try
//            {
//                var getEntity1Res = repo2.GetById("1");
//            }
//            catch (KeyNotFoundNoSQLException)
//            {
//                Assert.Fail("Should not raise KeyNotFoundNoSQLException");
//            }

//            // Delete from Repo 2 an entity Inserted in Repo 1
//            await repo2.Delete("1");

//            // Get from Repo 1 an entity Deleted in Repo 1
//            Exception exRes = null;
//            try
//            {
//                var getEntity1Res = repo1.GetById("1");
//                Assert.Fail("Repo1 should raise KeyNotFoundNoSQLException");
//            }
//            catch (Exception ex)
//            {
//                exRes = ex;
//            }


//            // Get from Repo 2 an entity Inserted in Repo 1
//            var entity2FromRepo1 = await repo1.GetById("2");
//            var entity2FromRepo2 = await repo2.GetById("2");

//            entity2FromRepo1.Name = "NameUpdatedInRepo1";
//            await repo1.Update(entity2FromRepo1);

//            Assert.AreNotEqual("NameUpdatedInRepo1", entity2FromRepo2, "Object instance from Repo 2 should not be affected");
            
//            var entity2FromRepo2AfterUpdate = await repo2.GetById("2");
//            Assert.AreEqual("NameUpdatedInRepo1", entity2FromRepo2AfterUpdate.Name, "Object instance from Repo 2 should have been updated with Repo 1 modification");

//        }
        
//        public virtual async Task ViewTests()
//        {
//            await entityRepo.TruncateCollection();

//            //
//            // Add test data
//            //
//            var entity1 = TestHelper.GetEntity1();
//            entity1.Id = "1";
//            await entityRepo.InsertOne(entity1);

//            var entity2 = TestHelper.GetEntity2();
//            entity2.Id = "2";
//            await entityRepo.InsertOne(entity2);

//            // Add the 3td et 4th entities to en secondary repo to ensure insert are visible throw all repositories
//            var entity3 = TestHelper.GetEntity3();
//            entity3.Id = "3";
//            await entityRepo2.InsertOne(entity3);

//            var entity4 = TestHelper.GetEntity4();
//            entity4.Id = "4";
//            await entityRepo2.InsertOne(entity4);

//            //
//            // Get data from an "Int" field
//            //

//            // Filter on 1 value
//            var task1 = await entityRepo.GetByField<int>(nameof(TestEntity.NumberOfChildenInt), 0);
//            var res1 = task1.OrderBy(e => e.Id).ToList();

//            Assert.AreEqual(2, res1.Count);
//            Assert.AreEqual("2", res1[0].Id);
//            Assert.AreEqual("3", res1[1].Id);
//            AssertHelper.AreJsonEqual(entity2, res1[0]);
//            AssertHelper.AreJsonEqual(entity3, res1[1]);

//            var task2 = await entityRepo.GetByField<int>(nameof(TestEntity.NumberOfChildenInt), 0);
//            var res2 = task2.OrderBy(e => e.Id).ToList();
//            Assert.AreEqual(2, res2.Count, "Check the an error not occured after a 2 call (the object entities are in memory)");

//            var task3 = await entityRepo.GetKeyByField<int>(nameof(TestEntity.NumberOfChildenInt), 0);
//            var res3 = task3.OrderBy(e => e).ToList();
//            Assert.AreEqual("2", res3[0]);
//            Assert.AreEqual("3", res3[1]);

//            Exception expectedEx = null;
//            try
//            {
//                var taskResult = await entityRepo.GetByField<int>(nameof(TestEntity.NumberOfChildenLong), 0);
//                taskResult.OrderBy(e => e.Id).ToList();
//            }
//            catch (Exception ex)
//            {

//                expectedEx = ex;
//            }
//            Assert.IsInstanceOfType(expectedEx, typeof(IndexNotFoundNoSQLException));

//            // Filter on a set of value

//            var searchedValues = new List<int> { 0, 10 };
//            var task4 = await entityRepo.GetByField<int>(nameof(TestEntity.NumberOfChildenInt), searchedValues);
//            var res4 = task4.OrderBy(e => e.Id).ToList();
//            Assert.AreEqual(3, res4.Count);

//            var task5 = await entityRepo.GetKeyByField<int>(nameof(TestEntity.NumberOfChildenInt), searchedValues);
//            var res5 = task5.OrderBy(e => e).ToList();
//            Assert.AreEqual(3, res5.Count);

//            //
//            // Get data from a "List<string>" field
//            //
//            var task6 = await entityRepo.GetByField<string>(nameof(TestEntity.Cities), "Grenoble");
//            var res6 = task6.OrderBy(e => e.Id).ToList();
//            Assert.AreEqual(3, res6.Where(e => e.Cities.Contains("Grenoble")).Count());

//            var searchList = new List<string> { "Grenoble", "Andernos" };
//            var task7 = await entityRepo.GetByField<string>(nameof(TestEntity.Cities), searchList);
//            var res7 = task7.OrderBy(e => e.Id).ToList();
//            Assert.AreEqual(3, res7.Count, "Dupplicate entries should be removed");
//            Assert.AreEqual(3, res7.Where(e => e.Cities.Contains("Grenoble")).Count());
//            Assert.AreEqual(1, res7.Where(e => e.Cities.Contains("Andernos")).Count());

//            var task8 = await entityRepo.GetByField<string>(nameof(TestEntity.Cities), "Grenoble");
//            var res8 = task8.OrderBy(e => e.Id).ToList();
//            Assert.AreEqual(3, res8.Count);

//            var task9 = await entityRepo.GetKeyByField<string>(nameof(TestEntity.Cities), searchList);
//            var res9 = task9.OrderBy(e => e).ToList();
//            Assert.AreEqual(3, res9.Count, "Dupplicate entries should be removed");

//            var task10 = await entityRepo.GetByField<string>(nameof(TestEntity.Cities), "GrEnObLe");
//            var res10 = task10.OrderBy(e => e.Id).ToList();
//            Assert.AreEqual(0, res10.Count, "String comparison should be case sensitive");

//            var task11 = await entityRepo.GetByField<string>(nameof(TestEntity.Cities), "Grenoblé");
//            var res11 = task11.OrderBy(e => e.Id).ToList();
//            Assert.AreEqual(0, res11.Count, "String comparison should be accent sensitive");

//        }

//        #endregion

//        #region Private

//        // Merged From linked CopyStream below and Jon Skeet's ReadFully example
//        private bool CompareStreams(Stream input1, Stream input2)
//        {
//            byte[] buffer1 = new byte[16 * 1024];
//            byte[] buffer2 = new byte[16 * 1024];

//            int read;
//            while ((read = input1.Read(buffer1, 0, buffer1.Length)) > 0)
//            {
//                input2.Read(buffer2, 0, buffer2.Length);

//                if (!buffer1.SequenceEqual(buffer2))
//                    return false;
//            }

//            return true;
//        }

//        private void InsertEntities(int nbEntities, int firstId, IAsyncNoSQLRepository<TestEntity> repo)
//        {
//            for (int i = 1; i <= nbEntities; i++)
//            {
//                Console.WriteLine(i);
//                TestEntity e = new TestEntity
//                {
//                    Id = (firstId + i).ToString(),
//                    PoidsDouble = Faker.RandomNumber.Next(),
//                    NumberOfChildenInt = Faker.RandomNumber.Next(),
//                    Name = Faker.Name.FullName()
//                };

//                repo.InsertOne(e);
//            }
//        }

//        #endregion


//    }
//}
