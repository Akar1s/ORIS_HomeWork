using System.Security.Cryptography;
using System.Text;

namespace TourSearch.Infrastructure;

public static class PasswordHasher
{
    private const int SaltSize = 16; 
    public static string GenerateSalt()
    {
        var bytes = new byte[SaltSize];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes);
    }

    public static string HashPassword(string password, string salt)
    {
        var combined = Encoding.UTF8.GetBytes(password + salt);
        using var sha = SHA256.Create();
        var hash = sha.ComputeHash(combined);
        return Convert.ToBase64String(hash);
    }

    public static bool VerifyPassword(string password, string salt, string expectedHash)
    {
        var actual = HashPassword(password, salt);
        return SlowEquals(Convert.FromBase64String(actual), Convert.FromBase64String(expectedHash));
    }

    private static bool SlowEquals(byte[] a, byte[] b)
    {
        uint diff = (uint)a.Length ^ (uint)b.Length;
        for (int i = 0; i < a.Length && i < b.Length; i++)
            diff |= (uint)(a[i] ^ b[i]);
        return diff == 0;
    }
}
