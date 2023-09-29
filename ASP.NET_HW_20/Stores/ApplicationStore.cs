using ASP.NET_HW_20.Data;
using Dapper;

namespace ASP.NET_HW_20.Stores;

public abstract class ApplicationStore {
    private readonly IDatabaseConnection _databaseConnection;

    protected ApplicationStore(IDatabaseConnection databaseConnection) {
        _databaseConnection = databaseConnection;
    }

    protected async Task<int> ExecuteQueryAsync(string query, object parameters) {
        await using var connection = _databaseConnection.GetConnection;
        return await connection.ExecuteAsync(query, parameters);
    }

    protected async Task<T> GetSingleOrDefaultAsync<T>(string query, object parameters) {
        await using var connection = _databaseConnection.GetConnection;
        var result = await connection.QuerySingleOrDefaultAsync<T>(query, parameters);
        return result;
    }

    protected async Task<IEnumerable<T>> GetIEnumerable<T>(string query, object parameters) {
        await using var connection = _databaseConnection.GetConnection;
        var result = await connection.QueryAsync<T>(query, parameters);
        return result;
    }
}