
using System.Security.Cryptography;
using System.Text;

namespace TourSearch.Infrastructure;

public static class PasswordResetTokenHelper
{
    private const string Secret = "password_reset_secret_change_me_in_production";

    public static string GenerateToken(int userId)
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var data = $"{userId}:{timestamp}:{Secret}";

        using var sha = SHA256.Create();
        var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(data));
        var hashHex = Convert.ToHexString(hash);

        return $"{userId}:{timestamp}:{hashHex}";
    }

    public static bool ValidateToken(string token, out int userId, out long timestamp)
    {
        userId = 0;
        timestamp = 0;

        var parts = token.Split(':', 3);
        if (parts.Length != 3) return false;

        if (!int.TryParse(parts[0], out userId)) return false;
        if (!long.TryParse(parts[1], out timestamp)) return false;

        var expected = GenerateTokenForValidation(userId, timestamp);
        return string.Equals(token, expected, StringComparison.OrdinalIgnoreCase);
    }

    private static string GenerateTokenForValidation(int userId, long timestamp)
    {
        var data = $"{userId}:{timestamp}:{Secret}";

        using var sha = SHA256.Create();
        var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(data));
        var hashHex = Convert.ToHexString(hash);

        return $"{userId}:{timestamp}:{hashHex}";
    }
}
