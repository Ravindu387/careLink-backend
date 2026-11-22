using CareLink.Infrastructure.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace CareLink.Tests.Common;

public static class TestDbContextFactory
{
    public static (ApplicationDbContext Context, SqliteConnection Connection) CreateSqliteInMemory()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connection)
            .EnableSensitiveDataLogging()
            .Options;

        var context = new ApplicationDbContext(options);
        context.Database.EnsureCreated();

        return (context, connection);
    }

    public static ApplicationDbContext CreateInMemory(string databaseName = "")
    {
        var dbName = string.IsNullOrWhiteSpace(databaseName) ? Guid.NewGuid().ToString() : databaseName;
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(dbName)
            .EnableSensitiveDataLogging()
            .Options;

        return new ApplicationDbContext(options);
    }
}
