using Npgsql;
using TourSearch.Data.Orm;
using TourSearch.Domain.Entities;

namespace TourSearch.Data;

public class DestinationRepository
{
    private readonly IDbSessionFactory _sessionFactory;

    public DestinationRepository(IDbSessionFactory sessionFactory)
    {
        _sessionFactory = sessionFactory;
    }

    public async Task<List<Destination>> GetAllAsync()
    {
        const string sql = @"
            SELECT id, name, country, description
            FROM destinations
            ORDER BY name;
        ";

        await using var session = _sessionFactory.CreateSession();

        return await session.QueryAsync(
            sql,
            Array.Empty<SqlParameter>(),
            MapDestination);
    }

    public async Task<Destination?> GetByIdAsync(int id)
    {
        const string sql = @"
            SELECT id, name, country, description
            FROM destinations
            WHERE id = @id;
        ";

        await using var session = _sessionFactory.CreateSession();

        var list = await session.QueryAsync(
            sql,
            new[] { new SqlParameter("id", id) },
            MapDestination);

        return list.FirstOrDefault();
    }

    public async Task<int> CreateAsync(Destination destination)
    {
        const string sql = @"
            INSERT INTO destinations (name, country, description)
            VALUES (@name, @country, @desc)
            RETURNING id;
        ";

        await using var session = _sessionFactory.CreateSession();

        var list = await session.QueryAsync(
            sql,
            new[] 
            { 
                new SqlParameter("name", destination.Name),
                new SqlParameter("country", destination.Country),
                new SqlParameter("desc", destination.Description)
            },
            reader => reader.GetInt32(0));

        return list.FirstOrDefault();
    }

    private static Destination MapDestination(NpgsqlDataReader reader)
    {
        return new Destination
        {
            Id = reader.GetInt32(0),
            Name = reader.GetString(1),
            Country = reader.GetString(2),
            Description = reader.IsDBNull(3) ? null : reader.GetString(3)
        };
    }
}
