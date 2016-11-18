namespace NoSqlRepositories.Core
{
    public enum UpdateMode
    {
        /// <summary>
        /// Use the native implementation of the DB
        /// </summary>
        db_implementation = 0,

        /// <summary>
        /// No exception will be raised
        /// </summary>
        do_nothing_if_missing_key = 1,

        /// <summary>
        /// A KeyNotFoundException.cs will be raised
        /// </summary>
        error_if_missing_key = 2,

        /// <summary>
        /// Insert a new record if it doesn't exists
        /// </summary>
        upsert_if_missing_key = 3
    }
}