namespace TourSearch.Data.Orm;

public class DbSessionFactory : IDbSessionFactory
{
    private readonly string _connectionString;

    public DbSessionFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    public IDbSession CreateSession()
    {
        return new DbSession(_connectionString);
    }
}
