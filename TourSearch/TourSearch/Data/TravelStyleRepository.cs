using Npgsql;
using TourSearch.Data.Orm;
using TourSearch.Domain.Entities;

namespace TourSearch.Data;

public class TravelStyleRepository
{
    private readonly IDbSessionFactory _sessionFactory;

    public TravelStyleRepository(IDbSessionFactory sessionFactory)
    {
        _sessionFactory = sessionFactory;
    }

    public async Task<List<TravelStyle>> GetAllAsync()
    {
        const string sql = @"
            SELECT id, name, description
            FROM travel_styles
            ORDER BY name;
        ";

        await using var session = _sessionFactory.CreateSession();

        return await session.QueryAsync(
            sql,
            Array.Empty<SqlParameter>(),
            MapStyle);
    }

    public async Task<TravelStyle?> GetByIdAsync(int id)
    {
        const string sql = @"
            SELECT id, name, description
            FROM travel_styles
            WHERE id = @id;
        ";

        await using var session = _sessionFactory.CreateSession();

        var list = await session.QueryAsync(
            sql,
            new[] { new SqlParameter("id", id) },
            MapStyle);

        return list.FirstOrDefault();
    }

    public async Task<int> CreateAsync(TravelStyle style)
    {
        const string sql = @"
            INSERT INTO travel_styles (name, description)
            VALUES (@name, @desc)
            RETURNING id;
        ";

        await using var session = _sessionFactory.CreateSession();

        var list = await session.QueryAsync(
            sql,
            new[] 
            { 
                new SqlParameter("name", style.Name),
                new SqlParameter("desc", style.Description)
            },
            reader => reader.GetInt32(0));

        return list.FirstOrDefault();
    }

    private static TravelStyle MapStyle(NpgsqlDataReader reader)
    {
        return new TravelStyle
        {
            Id = reader.GetInt32(0),
            Name = reader.GetString(1),
            Description = reader.IsDBNull(2) ? null : reader.GetString(2)
        };
    }
}
