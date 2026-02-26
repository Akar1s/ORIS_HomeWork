using System.Net;
using System.Security.Cryptography;
using System.Text;
using TourSearch.Data;
using TourSearch.Domain.Entities;

namespace TourSearch.Infrastructure;

public static class UserAuthHelper
{
    public static string GenerateToken(int userId)
    {
        var data = $"{userId}:{UserAuthConfig.AuthSecret}";
        using var sha = SHA256.Create();
        var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(data));
        var hashHex = Convert.ToHexString(hash);
        return $"{userId}:{hashHex}";
    }

    public static bool TryParseToken(string token, out int userId)
    {
        userId = 0;
        var parts = token.Split(':', 2);
        if (parts.Length != 2) return false;
        if (!int.TryParse(parts[0], out userId)) return false;

        var expected = GenerateToken(userId);
        return string.Equals(token, expected, StringComparison.OrdinalIgnoreCase);
    }

        public static async Task<User?> GetCurrentUserAsync(HttpListenerRequest request, UserRepository userRepo)
    {
        var cookie = request.Cookies[UserAuthConfig.AuthCookieName];
        if (cookie == null || string.IsNullOrEmpty(cookie.Value))
            return null;

        if (!TryParseToken(cookie.Value, out var userId))
            return null;

        return await userRepo.GetByIdAsync(userId);
    }
}
