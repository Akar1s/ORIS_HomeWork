namespace TourSearch.Data;

public static class DbConfig
{
    private static string GetEnvOrDefault(string key, string defaultValue)
    {
        return Environment.GetEnvironmentVariable(key) ?? defaultValue;
    }

    public static string ConnectionString
    {
        get
        {
            var host = GetEnvOrDefault("DB_HOST", "localhost");
            var port = GetEnvOrDefault("DB_PORT", "5432");
            var database = GetEnvOrDefault("DB_NAME", "toursearch");
            var username = GetEnvOrDefault("DB_USER", "postgres");
            var password = GetEnvOrDefault("DB_PASSWORD", "ntvh089");

            return $"Host={host};Port={port};Database={database};Username={username};Password={password}";
        }
    }
}
