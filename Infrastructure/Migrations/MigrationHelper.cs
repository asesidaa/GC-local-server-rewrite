using System.Data;
using Infrastructure.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Migrations;

public static class MigrationHelper
{
    public static bool Exists(string tableName)
    {
        var options = new DbContextOptionsBuilder().UseSqlite($"Data Source={GetConnectionString()}");
        using var context = new DbContext(options.Options);
        using var command = context.Database.GetDbConnection().CreateCommand();
        command.CommandText = $"SELECT count(*) FROM sqlite_master WHERE type='table' AND name='{tableName}';";
        command.CommandType = CommandType.Text;
        context.Database.OpenConnection();

        using var reader = command.ExecuteReader();
            
        return reader.Read()? (long)reader[0] == 1 : false;
    }
        
    public static string GetConnectionString()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(PathHelper.ConfigurationPath)
            .AddJsonFile("database.json", optional: false, reloadOnChange: false);

        var cardDbName = builder.Build()["CardDbName"];
        var cardDbPath = Path.Combine(PathHelper.DatabasePath, cardDbName);
        return cardDbPath;
    }
}