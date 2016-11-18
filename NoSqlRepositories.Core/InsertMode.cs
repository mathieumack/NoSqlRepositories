namespace NoSqlRepositories.Core
{
    public enum InsertMode
    {
        /// <summary>
        /// A DuplicateKeyException.cs will be raised
        /// </summary>
        error_if_key_exists = 0,

        /// <summary>
        /// No exception will be raised
        /// </summary>
        do_nothing_if_key_exists = 1,

        /// <summary>
        /// Existing records will be deleted and replaced by the new one
        /// </summary>
        erase_existing = 2,

        /// <summary>
        /// A DuplicateKeyException.cs will be raised
        /// </summary>
        db_implementation = 3
    }
}
