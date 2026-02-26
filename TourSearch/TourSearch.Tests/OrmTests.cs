using TourSearch.Data.Orm;
using Xunit;

namespace TourSearch.Tests;

public class SqlParameterTests
{
    [Fact]
    public void Constructor_SetsNameAndValue()
    {
                var param = new SqlParameter("userId", 42);

                Assert.Equal("userId", param.Name);
        Assert.Equal(42, param.Value);
    }

    [Fact]
    public void Constructor_AcceptsNullValue()
    {
                var param = new SqlParameter("email", null);

                Assert.Equal("email", param.Name);
        Assert.Null(param.Value);
    }

    [Fact]
    public void Constructor_AcceptsStringValue()
    {
                var param = new SqlParameter("name", "Test Tour");

                Assert.Equal("name", param.Name);
        Assert.Equal("Test Tour", param.Value);
    }

    [Fact]
    public void Constructor_AcceptsDecimalValue()
    {
                var param = new SqlParameter("price", 499.99m);

                Assert.Equal("price", param.Name);
        Assert.Equal(499.99m, param.Value);
    }

    [Fact]
    public void Constructor_AcceptsDateTimeValue()
    {
                var date = new DateTime(2025, 6, 15);
        
                var param = new SqlParameter("startDate", date);

                Assert.Equal("startDate", param.Name);
        Assert.Equal(date, param.Value);
    }
}

public class DbSessionFactoryTests
{
    [Fact]
    public void CreateSession_ReturnsDbSession()
    {
                var connectionString = "Host=localhost;Database=test;Username=user;Password=pass";
        var factory = new DbSessionFactory(connectionString);

                var session = factory.CreateSession();

                Assert.NotNull(session);
        Assert.IsType<DbSession>(session);
    }

    [Fact]
    public void CreateSession_MultipleCallsReturnDifferentSessions()
    {
                var connectionString = "Host=localhost;Database=test;Username=user;Password=pass";
        var factory = new DbSessionFactory(connectionString);

                var session1 = factory.CreateSession();
        var session2 = factory.CreateSession();

                Assert.NotSame(session1, session2);
    }
}

public class PasswordHasherTests
{
    [Fact]
    public void GenerateSalt_ReturnsNonEmptyString()
    {
                var salt = TourSearch.Infrastructure.PasswordHasher.GenerateSalt();

                Assert.False(string.IsNullOrWhiteSpace(salt));
    }

    [Fact]
    public void GenerateSalt_ReturnsDifferentValuesEachTime()
    {
                var salt1 = TourSearch.Infrastructure.PasswordHasher.GenerateSalt();
        var salt2 = TourSearch.Infrastructure.PasswordHasher.GenerateSalt();

                Assert.NotEqual(salt1, salt2);
    }

    [Fact]
    public void HashPassword_ReturnsSameHashForSameSalt()
    {
                var password = "TestPassword123";
        var salt = TourSearch.Infrastructure.PasswordHasher.GenerateSalt();

                var hash1 = TourSearch.Infrastructure.PasswordHasher.HashPassword(password, salt);
        var hash2 = TourSearch.Infrastructure.PasswordHasher.HashPassword(password, salt);

                Assert.Equal(hash1, hash2);
    }

    [Fact]
    public void HashPassword_ReturnsDifferentHashForDifferentSalt()
    {
                var password = "TestPassword123";
        var salt1 = TourSearch.Infrastructure.PasswordHasher.GenerateSalt();
        var salt2 = TourSearch.Infrastructure.PasswordHasher.GenerateSalt();

                var hash1 = TourSearch.Infrastructure.PasswordHasher.HashPassword(password, salt1);
        var hash2 = TourSearch.Infrastructure.PasswordHasher.HashPassword(password, salt2);

                Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void VerifyPassword_ReturnsTrueForCorrectPassword()
    {
                var password = "TestPassword123";
        var salt = TourSearch.Infrastructure.PasswordHasher.GenerateSalt();
        var hash = TourSearch.Infrastructure.PasswordHasher.HashPassword(password, salt);

                var result = TourSearch.Infrastructure.PasswordHasher.VerifyPassword(password, salt, hash);

                Assert.True(result);
    }

    [Fact]
    public void VerifyPassword_ReturnsFalseForIncorrectPassword()
    {
                var password = "TestPassword123";
        var wrongPassword = "WrongPassword123";
        var salt = TourSearch.Infrastructure.PasswordHasher.GenerateSalt();
        var hash = TourSearch.Infrastructure.PasswordHasher.HashPassword(password, salt);

                var result = TourSearch.Infrastructure.PasswordHasher.VerifyPassword(wrongPassword, salt, hash);

                Assert.False(result);
    }
}
