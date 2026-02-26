using Npgsql;
using TourSearch.Data.Orm;
using TourSearch.Domain.Entities;

namespace TourSearch.Data;

public class TourRepository
{
    private readonly IDbSessionFactory _sessionFactory;

    public TourRepository(IDbSessionFactory sessionFactory)
    {
        _sessionFactory = sessionFactory;
    }

    public async Task<List<Tour>> GetAllAsync()
    {
        const string sql = @"
            SELECT id, name, duration_days, base_price, start_date,
                   destination_id, travel_style_id, description, itinerary, whats_included, image_url
            FROM tours
            ORDER BY id;
        ";

        await using var session = _sessionFactory.CreateSession();

        return await session.QueryAsync(
            sql,
            Array.Empty<SqlParameter>(),
            MapTour);
    }

    public async Task<List<TourWithDetails>> GetAllWithDetailsAsync()
    {
        const string sql = @"
            SELECT t.id, t.name, t.duration_days, t.base_price, t.start_date,
                   t.destination_id, t.travel_style_id,
                   d.name as destination_name,
                   ts.name as travel_style_name,
                   t.description, t.itinerary, t.whats_included, t.image_url
            FROM tours t
            LEFT JOIN destinations d ON t.destination_id = d.id
            LEFT JOIN travel_styles ts ON t.travel_style_id = ts.id
            ORDER BY t.id;
        ";

        await using var session = _sessionFactory.CreateSession();

        return await session.QueryAsync(
            sql,
            Array.Empty<SqlParameter>(),
            MapTourWithDetails);
    }

    private static Tour MapTour(NpgsqlDataReader reader)
    {
        return new Tour
        {
            Id = reader.GetInt32(0),
            Name = reader.GetString(1),
            DurationDays = reader.GetInt32(2),
            BasePrice = reader.GetDecimal(3),
            StartDate = reader.IsDBNull(4) ? null : reader.GetDateTime(4),
            DestinationId = reader.GetInt32(5),
            TravelStyleId = reader.GetInt32(6),
            Description = reader.IsDBNull(7) ? null : reader.GetString(7),
            Itinerary = reader.IsDBNull(8) ? null : reader.GetString(8),
            WhatsIncluded = reader.IsDBNull(9) ? null : reader.GetString(9),
            ImageUrl = reader.IsDBNull(10) ? null : reader.GetString(10)
        };
    }

    private static TourWithDetails MapTourWithDetails(NpgsqlDataReader reader)
    {
        return new TourWithDetails
        {
            Id = reader.GetInt32(0),
            Name = reader.GetString(1),
            DurationDays = reader.GetInt32(2),
            BasePrice = reader.GetDecimal(3),
            StartDate = reader.IsDBNull(4) ? null : reader.GetDateTime(4),
            DestinationId = reader.GetInt32(5),
            TravelStyleId = reader.GetInt32(6),
            DestinationName = reader.IsDBNull(7) ? null : reader.GetString(7),
            TravelStyleName = reader.IsDBNull(8) ? null : reader.GetString(8),
            Description = reader.IsDBNull(9) ? null : reader.GetString(9),
            Itinerary = reader.IsDBNull(10) ? null : reader.GetString(10),
            WhatsIncluded = reader.IsDBNull(11) ? null : reader.GetString(11),
            ImageUrl = reader.IsDBNull(12) ? null : reader.GetString(12)
        };
    }

    public async Task<Tour?> GetByIdAsync(int id)
    {
        const string sql = @"
            SELECT id, name, duration_days, base_price, start_date,
                   destination_id, travel_style_id, description, itinerary, whats_included, image_url
            FROM tours
            WHERE id = @id;
        ";

        await using var session = _sessionFactory.CreateSession();

        var list = await session.QueryAsync(
            sql,
            new[] { new SqlParameter("id", id) },
            MapTour);

        return list.FirstOrDefault();
    }
    
    public async Task<TourWithDetails?> GetByIdWithDetailsAsync(int id)
    {
        const string sql = @"
            SELECT t.id, t.name, t.duration_days, t.base_price, t.start_date,
                   t.destination_id, t.travel_style_id,
                   d.name as destination_name,
                   ts.name as travel_style_name,
                   t.description, t.itinerary, t.whats_included, t.image_url
            FROM tours t
            LEFT JOIN destinations d ON t.destination_id = d.id
            LEFT JOIN travel_styles ts ON t.travel_style_id = ts.id
            WHERE t.id = @id;
        ";

        await using var session = _sessionFactory.CreateSession();

        var list = await session.QueryAsync(
            sql,
            new[] { new SqlParameter("id", id) },
            MapTourWithDetails);

        return list.FirstOrDefault();
    }

    public async Task<int> CreateAsync(Tour tour)
    {
        const string sql = @"
            INSERT INTO tours
                (name, duration_days, base_price, start_date, destination_id, travel_style_id, description, itinerary, whats_included, image_url)
            VALUES
                (@name, @days, @price, @start_date, @dest_id, @style_id, @desc, @itinerary, @included, @image_url);
        ";

        await using var session = _sessionFactory.CreateSession();

        var parameters = new[]
        {
            new SqlParameter("name", tour.Name),
            new SqlParameter("days", tour.DurationDays),
            new SqlParameter("price", tour.BasePrice),
            new SqlParameter("start_date", tour.StartDate),
            new SqlParameter("dest_id", tour.DestinationId),
            new SqlParameter("style_id", tour.TravelStyleId),
            new SqlParameter("desc", tour.Description),
            new SqlParameter("itinerary", tour.Itinerary),
            new SqlParameter("included", tour.WhatsIncluded),
            new SqlParameter("image_url", tour.ImageUrl)
        };

        return await session.ExecuteAsync(sql, parameters);
    }

    public async Task<int> UpdateAsync(Tour tour)
    {
        const string sql = @"
            UPDATE tours
            SET name = @name,
                duration_days = @days,
                base_price = @price,
                start_date = @start_date,
                destination_id = @dest_id,
                travel_style_id = @style_id,
                description = @desc,
                itinerary = @itinerary,
                whats_included = @included,
                image_url = @image_url
            WHERE id = @id;
        ";

        await using var session = _sessionFactory.CreateSession();

        var parameters = new[]
        {
            new SqlParameter("id", tour.Id),
            new SqlParameter("name", tour.Name),
            new SqlParameter("days", tour.DurationDays),
            new SqlParameter("price", tour.BasePrice),
            new SqlParameter("start_date", tour.StartDate),
            new SqlParameter("dest_id", tour.DestinationId),
            new SqlParameter("style_id", tour.TravelStyleId),
            new SqlParameter("desc", tour.Description),
            new SqlParameter("itinerary", tour.Itinerary),
            new SqlParameter("included", tour.WhatsIncluded),
            new SqlParameter("image_url", tour.ImageUrl)
        };

        return await session.ExecuteAsync(sql, parameters);
    }

    public async Task<int> DeleteAsync(int id)
    {
        const string sql = @"DELETE FROM tours WHERE id = @id;";

        await using var session = _sessionFactory.CreateSession();

        var parameters = new[]
        {
            new SqlParameter("id", id)
        };

        return await session.ExecuteAsync(sql, parameters);
    }
}

public class TourWithDetails
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int DurationDays { get; set; }
    public decimal BasePrice { get; set; }
    public DateTime? StartDate { get; set; }
    public int DestinationId { get; set; }
    public int TravelStyleId { get; set; }
    public string? DestinationName { get; set; }
    public string? TravelStyleName { get; set; }
    public string? Description { get; set; }
    public string? Itinerary { get; set; }
    public string? WhatsIncluded { get; set; }
    public string? ImageUrl { get; set; }
}
