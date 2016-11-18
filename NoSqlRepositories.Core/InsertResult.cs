namespace NoSqlRepositories.Core
{
    public enum InsertResult
    {
        unknown = 0,
        not_affected = 1,
        inserted = 2,
        updated = 3,
        duplicate_key_exception = 4,
        other_exception = 5
    }
}