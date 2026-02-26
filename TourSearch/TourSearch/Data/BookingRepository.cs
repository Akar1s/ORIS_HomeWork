using TourSearch.Data.Orm;
using TourSearch.Domain.Entities;

namespace TourSearch.Data;

public class BookingRepository
{
    private readonly IDbSessionFactory _sessionFactory;

    public BookingRepository(IDbSessionFactory sessionFactory)
    {
        _sessionFactory = sessionFactory;
    }

    public async Task<int> CreateAsync(Booking booking)
    {
        const string sql = @"
            INSERT INTO bookings
                (tour_id, customer_name, customer_email, persons_count, status, created_at)
            VALUES
                (@tour_id, @name, @email, @persons, @status, @created_at);
        ";

        await using var session = _sessionFactory.CreateSession();

        var parameters = new[]
        {
            new SqlParameter("tour_id", booking.TourId),
            new SqlParameter("name", booking.CustomerName),
            new SqlParameter("email", booking.CustomerEmail),
            new SqlParameter("persons", booking.PersonsCount),
            new SqlParameter("status", booking.Status),
            new SqlParameter("created_at", booking.CreatedAt)
        };

        return await session.ExecuteAsync(sql, parameters);     }
}
