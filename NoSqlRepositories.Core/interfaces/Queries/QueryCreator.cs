//using System;

//namespace NoSqlRepositories.Core.Queries
//{
//    public static class QueryCreator
//    {
//        /// <summary>
//        /// Create a new NoSqlQuery object to be used on Query
//        /// </summary>
//        /// <typeparam name="T"></typeparam>
//        /// <param name="limit"></param>
//        /// <param name="skip"></param>
//        /// <param name="postFilter"></param>
//        /// <returns></returns>
//        public static NoSqlQuery<T> CreateQueryOptions<T>(int limit, 
//                                                            int skip, 
//                                                            Func<T, bool> postFilter) where T : IBaseEntity
//        {
//            return new NoSqlQuery<T>()
//            {
//                Limit = limit,
//                Skip = skip,
//                PostFilter = postFilter
//            };
//        }
//    }
//}
