﻿using System;
using System.Collections.Generic;

namespace NoSqlRepositories.Core.Helpers
{
    public static class NoSQLRepoHelper
    {
        // Datetime.UtcNow lambda, which can be overided to permit unit testing
        public static Func<DateTimeOffset> DateTimeUtcNow { get; set; } = (() => { return new DateTimeOffset(DateTime.Now.ToUniversalTime()); });

        // Datetime.Now lambda, which can be overided to permit unit testing
        public static Func<DateTimeOffset> DateTimeNow { get; set; } = (() => { return new DateTimeOffset(DateTime.Now.ToUniversalTime()); });

        /// <summary>
        /// List of the field that should be ingored when comparing two entity version to determine if the entity has been modified
        /// </summary>
        public static List<string> IgnoredFieldMapping { get; set; } = new List<string> { };

        /// <summary>
        /// Define internal _DbId and DocId if they are not specified by user
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        public static void SetIds<T>(T entity) where T : IBaseEntity
        {

            if (string.IsNullOrWhiteSpace(entity.Id))
            {
                entity.Id = GetNewGuid();
            }
        }

        private static string GetNewGuid()
        {
            return Guid.NewGuid().ToString();
        }
    }
}
