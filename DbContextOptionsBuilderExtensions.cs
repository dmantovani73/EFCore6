﻿static class DbContextOptionsBuilderExtensions
{
    public static DbContextOptionsBuilder<T> Use<T>(this DbContextOptionsBuilder<T> builder, DbProvider provider, string connectionString)
        where T : DbContext
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(connectionString);

        return provider switch
        {
            SQLite => builder.UseSqlite(connectionString),
            SQLServer => builder.UseSqlServer(connectionString),
            PostgreSQL => builder.UseNpgsql(connectionString),
            _ => throw new NotSupportedException(),
        };
    }
}

enum DbProvider
{
    SQLite,
    SQLServer,
    PostgreSQL,
}
