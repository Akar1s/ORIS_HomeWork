using Npgsql;
using TourSearch.Data.Orm;
using TourSearch.Domain.Entities;

namespace TourSearch.Data;

public class UserRepository
{
    private readonly IDbSessionFactory _sessionFactory;

    public UserRepository(IDbSessionFactory sessionFactory)
    {
        _sessionFactory = sessionFactory;
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        const string sql = @"
            SELECT id, email, password_hash, salt, role, created_at
            FROM users
            WHERE email = @email;
        ";

        await using var session = _sessionFactory.CreateSession();

        var list = await session.QueryAsync(
            sql,
            new[] { new SqlParameter("email", email) },
            MapUser);

        return list.FirstOrDefault();
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        const string sql = @"
            SELECT id, email, password_hash, salt, role, created_at
            FROM users
            WHERE id = @id;
        ";

        await using var session = _sessionFactory.CreateSession();

        var list = await session.QueryAsync(
            sql,
            new[] { new SqlParameter("id", id) },
            MapUser);

        return list.FirstOrDefault();
    }

    public async Task<int> CreateAsync(User user)
    {
        const string sql = @"
            INSERT INTO users (email, password_hash, salt, role, created_at)
            VALUES (@email, @hash, @salt, @role, @created_at);
        ";

        await using var session = _sessionFactory.CreateSession();

        var parameters = new[]
        {
            new SqlParameter("email", user.Email),
            new SqlParameter("hash", user.PasswordHash),
            new SqlParameter("salt", user.Salt),
            new SqlParameter("role", user.Role),
            new SqlParameter("created_at", user.CreatedAt)
        };

        return await session.ExecuteAsync(sql, parameters);
    }

    private static User MapUser(NpgsqlDataReader reader)
    {
        return new User
        {
            Id = reader.GetInt32(0),
            Email = reader.GetString(1),
            PasswordHash = reader.GetString(2),
            Salt = reader.GetString(3),
            Role = reader.GetString(4),
            CreatedAt = reader.GetDateTime(5)
        };
    }
    public async Task SavePasswordResetTokenAsync(int userId, string token, DateTime expiresAt)
    {
        const string sql = @"
        INSERT INTO password_reset_tokens (user_id, token, expires_at, created_at)
        VALUES (@user_id, @token, @expires_at, @created_at)
        ON CONFLICT (user_id) 
        DO UPDATE SET token = @token, expires_at = @expires_at, created_at = @created_at;
    ";

        await using var session = _sessionFactory.CreateSession();

        var parameters = new[]
        {
        new SqlParameter("user_id", userId),
        new SqlParameter("token", token),
        new SqlParameter("expires_at", expiresAt),
        new SqlParameter("created_at", DateTime.UtcNow)
    };

        await session.ExecuteAsync(sql, parameters);
    }

    public async Task<int?> ValidatePasswordResetTokenAsync(string token)
    {
        const string sql = @"
        SELECT user_id FROM password_reset_tokens
        WHERE token = @token AND expires_at > @now;
    ";

        await using var session = _sessionFactory.CreateSession();

        var list = await session.QueryAsync(
            sql,
            new[]
            {
            new SqlParameter("token", token),
            new SqlParameter("now", DateTime.UtcNow)
            },
            reader => reader.GetInt32(0)
        );

        return list.FirstOrDefault() == 0 ? null : list.FirstOrDefault();
    }

    public async Task DeletePasswordResetTokenAsync(string token)
    {
        const string sql = @"DELETE FROM password_reset_tokens WHERE token = @token;";

        await using var session = _sessionFactory.CreateSession();
        await session.ExecuteAsync(sql, new[] { new SqlParameter("token", token) });
    }

    public async Task<int> UpdateAsync(User user)
    {
        const string sql = @"
        UPDATE users
        SET email = @email,
            password_hash = @hash,
            salt = @salt,
            role = @role
        WHERE id = @id;
    ";

        await using var session = _sessionFactory.CreateSession();

        var parameters = new[]
        {
        new SqlParameter("id", user.Id),
        new SqlParameter("email", user.Email),
        new SqlParameter("hash", user.PasswordHash),
        new SqlParameter("salt", user.Salt),
        new SqlParameter("role", user.Role)
    };

        return await session.ExecuteAsync(sql, parameters);
    }

}
