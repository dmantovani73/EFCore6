
internal static class ConnectionStrings
{
    static readonly Dictionary<DbProvider, string> configurations = new()
    {
        [SQLite] = @"Data Source=..\..\..\University.db;",
        [SQLServer] = "Server=127.0.0.1; Database=University; User ID=sa; Password=Pa$$w0rd; Application Name=EFCore6;",
        [PostgreSQL] = "Host=localhost; Database=University; User ID=postgres; Password=Pa$$w0rd;",
    };

    public static string Get(DbProvider provider) => configurations[provider];
}
