namespace TourSearch.Data.Orm;

public interface IDbSession : IAsyncDisposable
{
    Task<List<T>> QueryAsync<T>(string sql, IEnumerable<SqlParameter> parameters, Func<Npgsql.NpgsqlDataReader, T> map);
    Task<int> ExecuteAsync(string sql, IEnumerable<SqlParameter> parameters);
}
