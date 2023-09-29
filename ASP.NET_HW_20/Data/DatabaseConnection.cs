using Microsoft.Data.Sqlite;

namespace ASP.NET_HW_20.Data;

public class DatabaseConnection : IDatabaseConnection {
    private readonly IConfiguration _configuration;

    public DatabaseConnection(IConfiguration configuration) {
        _configuration = configuration;
    }

    public SqliteConnection GetConnection {
        get {
            var connectionString = _configuration.GetConnectionString("DefaultConnection") ??
                                   throw new InvalidOperationException(
                                       "Connection string 'DefaultConnection' not found.");
            return new SqliteConnection(connectionString);
        }
    }
}