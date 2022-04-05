using SQLite.Net2;

namespace GCLocalServerRewrite.common;

public static class DatabaseHelper
{
    /// <summary>
    ///     Static method to allow local data services to initialise their associated database conveniently.
    /// </summary>
    /// <param name="databaseName">The SQLite database name</param>
    /// <param name="tables">The SQLite database tables to create (if required)</param>
    /// <returns>An initialised SQLite database connection</returns>
    public static SQLiteConnection InitializeLocalDatabase(string databaseName, params Type[] tables)
    {
        if (!Directory.Exists(PathHelper.DataBaseRootPath))
        {
            Directory.CreateDirectory(PathHelper.DataBaseRootPath);
        }

        var databasePath = Path.Combine(PathHelper.DataBaseRootPath, databaseName);

        var database = new SQLiteConnection(databasePath);

        foreach (var table in tables)
        {
            database.CreateTable(table);
        }

        return database;
    }

    public static SQLiteConnection ConnectDatabase(string databaseName)
    {
        var databasePath = Path.Combine(PathHelper.DataBaseRootPath, databaseName);

        var database = new SQLiteConnection(databasePath);

        return database;
    }
}