using Npgsql;
using TourSearch.Infrastructure;

namespace TourSearch.Data.Orm;

public class DbSession : IDbSession
{
    private readonly NpgsqlConnection _connection;
    private bool _disposed;

    public DbSession(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string cannot be null or empty", nameof(connectionString));
            
        _connection = new NpgsqlConnection(connectionString);
    }

    public async Task OpenAsync()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(DbSession));
            
        try
        {
            if (_connection.State != System.Data.ConnectionState.Open)
                await _connection.OpenAsync();
        }
        catch (NpgsqlException ex)
        {
            Logger.Error(ex, "Failed to open database connection");
            throw new DatabaseException("Failed to open database connection", ex);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Unexpected error opening database connection");
            throw new DatabaseException("Unexpected error opening database connection", ex);
        }
    }

    public async Task<List<T>> QueryAsync<T>(
        string sql,
        IEnumerable<SqlParameter> parameters,
        Func<NpgsqlDataReader, T> map)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(DbSession));
        if (string.IsNullOrWhiteSpace(sql))
            throw new ArgumentException("SQL query cannot be null or empty", nameof(sql));
        if (map == null)
            throw new ArgumentNullException(nameof(map));

        try
        {
            await OpenAsync();

            await using var cmd = new NpgsqlCommand(sql, _connection);

            if (parameters != null)
            {
                foreach (var p in parameters)
                {
                    if (p != null)
                        cmd.Parameters.AddWithValue(p.Name, p.Value ?? DBNull.Value);
                }
            }

            var list = new List<T>();

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                try
                {
                    list.Add(map(reader));
                }
                catch (Exception ex)
                {
                    Logger.Warning($"Error mapping row: {ex.Message}");
                                    }
            }

            return list;
        }
        catch (NpgsqlException ex)
        {
            Logger.Error(ex, $"Database query error: {sql}");
            throw new DatabaseException($"Database query error", ex);
        }
        catch (ObjectDisposedException)
        {
            throw;
        }
        catch (DatabaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, $"Unexpected error in query: {sql}");
            throw new DatabaseException("Unexpected database error", ex);
        }
    }

    public async Task<int> ExecuteAsync(string sql, IEnumerable<SqlParameter> parameters)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(DbSession));
        if (string.IsNullOrWhiteSpace(sql))
            throw new ArgumentException("SQL query cannot be null or empty", nameof(sql));

        try
        {
            await OpenAsync();

            await using var cmd = new NpgsqlCommand(sql, _connection);
            
            if (parameters != null)
            {
                foreach (var p in parameters)
                {
                    if (p != null)
                        cmd.Parameters.AddWithValue(p.Name, p.Value ?? DBNull.Value);
                }
            }

            return await cmd.ExecuteNonQueryAsync();
        }
        catch (NpgsqlException ex)
        {
            Logger.Error(ex, $"Database execute error: {sql}");
            throw new DatabaseException($"Database execute error", ex);
        }
        catch (ObjectDisposedException)
        {
            throw;
        }
        catch (DatabaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, $"Unexpected error in execute: {sql}");
            throw new DatabaseException("Unexpected database error", ex);
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;
            
        _disposed = true;

        try
        {
            await _connection.CloseAsync();
        }
        catch (Exception ex)
        {
            Logger.Warning($"Error closing connection: {ex.Message}");
        }

        try
        {
            await _connection.DisposeAsync();
        }
        catch (Exception ex)
        {
            Logger.Warning($"Error disposing connection: {ex.Message}");
        }
    }
}

public class DatabaseException : Exception
{
    public DatabaseException(string message) : base(message) { }
    public DatabaseException(string message, Exception innerException) : base(message, innerException) { }
}
