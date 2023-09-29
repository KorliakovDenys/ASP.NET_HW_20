using Microsoft.Data.Sqlite;

namespace ASP.NET_HW_20.Data;

public interface IDatabaseConnection {
    public SqliteConnection GetConnection { get; }
}