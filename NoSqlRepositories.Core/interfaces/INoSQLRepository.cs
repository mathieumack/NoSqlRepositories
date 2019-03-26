﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using NoSqlRepositories.Core.interfaces;
using NoSqlRepositories.Core.Queries;

namespace NoSqlRepositories.Core
{
    public interface INoSQLRepository<T> : INoSQLDB where T : class, IBaseEntity, new()
    {
        /// <summary>
        /// Indicate if the connection is opened
        /// </summary>
        bool ConnectionOpened { get; }

        /// <summary>
        /// Get the entity corresponding to the provided id. Raise an IdNotFoundException if not found
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        INoSqlEntity<T> GetById(string id);

        /// <summary>
        /// Get the entity corresponding to the provided id. Return null if not found
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        INoSqlEntity<T> TryGetById(string id);

        /// <summary>
        /// Get the entities that match given ids. The list is empty if no entities are found
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        IEnumerable<INoSqlEntity<T>> GetByIds(IList<string> ids);

        /// <summary>
        /// Insert one entity
        /// Raise an error if the key is already used
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="insertMode"></param>
        /// <returns></returns>
        InsertResult InsertOne(INoSqlEntity<T> entity);

        /// <summary>
        /// Insert one entity with the specified insert mode
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="insertMode"></param>
        /// <returns></returns>
        InsertResult InsertOne(INoSqlEntity<T> entity, InsertMode insertMode);

        /// <summary>
        /// Insert a set of entities
        /// Default db implementation behavor if key exists 
        /// </summary>
        /// <param name="entities"></param>
        /// <param name="insertMode"></param>
        /// <returns></returns>
        BulkInsertResult<string> InsertMany(IEnumerable<INoSqlEntity<T>> entities);

        /// <summary>
        /// Insert a set of entities using the specified insert mode
        /// </summary>
        /// <param name="entities"></param>
        /// <param name="insertMode"></param>
        /// <returns></returns>
        BulkInsertResult<string> InsertMany(IEnumerable<INoSqlEntity<T>> entities, InsertMode insertMode);

        /// <summary>
        /// Create a new query on database.
        /// </summary>
        /// <returns></returns>
        // TODO : Reactivate queries
        //IEnumerable<INoSqlEntity<T>> DoQuery(NoSqlQuery<INoSqlEntity<T>> queryFilters);

        /// <summary>
        /// Test if the entity key exists in the repository
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        bool Exist(string id);

        /// <summary>
        /// Close the connection to the database
        /// </summary>
        /// <returns></returns>
        Task Close();

        /// <summary>
        /// Connect again to the database if you already call the Close method
        /// </summary>
        /// <returns></returns>
        void ConnectAgain();

        /// <summary>
        /// Update an entity with the specify behavor
        /// </summary>
        /// <param name="entity">The new version of the entity</param>
        /// <param name="isUpsert">Behavor of the update</param>
        /// <returns>Return number of affected entities</returns>
        UpdateResult Update(INoSqlEntity<T> entity, UpdateMode updateMode);

        /// <summary>
        /// Update an entity using the default db implementation
        /// </summary>
        /// <param name="entity">The new version of the entity</param>
        /// <returns>Return number of affected entities</returns>
        UpdateResult Update(INoSqlEntity<T> entity);

        /// <summary>
        /// Delete an entity
        /// </summary>
        /// <param name="id">Id of the entity</param>
        /// <returns>Number of entity deleted</returns>
        long Delete(string id);

        /// <summary>
        /// Delete an entity
        /// </summary>
        /// <param name="id">Id of the entity</param>
        /// <param name="physical">False to make a logical delete of the entity</param>
        /// <returns>Number of entity deleted</returns>
        long Delete(string id, bool physical);

        /// <summary>
        /// Set to false to Delegate to the client the affectation of creation and update date (before create/update operations)
        /// </summary>
        bool AutoGeneratedEntityDate { get; set; }

        /// <summary>
        /// Run initilization command of the collection (if required by the repository implementation)
        /// </summary>
        /// <returns>True if the collection has been initialized</returns>
        /// <returns></returns>
        void InitCollection(IList<string> indexFieldSelectors);

        /// <summary>
        /// Add an attachment to an entity
        /// </summary>
        /// <param name="id">id of entity</param>
        /// <param name="filePathAttachment">file path of the file to attach</param>
        /// <param name="contentType">type of the file to attach</param>
        /// <param name="attachmentName">Name of the file to attach. Unique identier of a file inside an entity.</param>
        void AddAttachment(string id, Stream fileStream, string contentType, string attachmentName);

        /// <summary>
        /// Remove an attachment of a document
        /// </summary>
        /// <param name="id">Id of entity</param>
        /// <param name="attachmentName">Name of the file to attach. Unique identier of a file inside an entity.</param>
        void RemoveAttachment(string id, string attachmentName);

        /// <summary>
        /// Create a new memory document.
        /// The created document is not inserted in database
        /// </summary>
        /// <returns></returns>
        INoSqlEntity<T> CreateNewDocument(T entity);

        /// <summary>
        /// Create a new memory document.
        /// The created document is not inserted in database
        /// </summary>
        /// <returns></returns>
        INoSqlEntity<T> CreateNewDocument();

        /// <summary>
        /// Create a new memory document.
        /// The created document is not inserted in database
        /// </summary>
        /// <param name="id">specified id</param>
        /// <returns></returns>
        INoSqlEntity<T> CreateNewDocument(string id);

        /// <summary>
        /// Get one attachment of a document
        /// </summary>
        /// <param name="id">Id of entity</param>
        /// <param name="attachmentName">Name of the file to attach. Unique identier of a file inside an entity.</param>
        Stream GetAttachment(string id, string attachmentName);

        /// <summary>
        /// Get one attachment of a document
        /// </summary>
        /// <param name="id">Id of entity</param>
        /// <param name="attachmentName">Name of the file to attach. Unique identier of a file inside an entity.</param>
        Byte[] GetByteAttachment(string id, string attachmentName);

        /// <summary>
        /// Get attachmentName of a given entity
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        IEnumerable<string> GetAttachmentNames(string id);

        /// <summary>
        /// Return all entities of the repository
        /// </summary>
        /// <returns></returns>
        IEnumerable<INoSqlEntity<T>> GetAll();

        /// <summary>
        /// Get entities Ids that match de field = List of Value condition
        /// </summary>
        /// <typeparam name="TField"></typeparam>
        /// <param name="fieldName"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        IEnumerable<string> GetKeyByField<TField>(string fieldName, List<TField> values);

        /// <summary>
        /// Get entities Ids that match de field = Value condition
        /// </summary>
        /// <typeparam name="TField"></typeparam>
        /// <param name="fieldName"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        IEnumerable<string> GetKeyByField<TField>(string fieldName, TField value);

        /// <summary>
        /// Get the number of entities
        /// </summary>
        /// <returns></returns>
        int Count();
    }
}
