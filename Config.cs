public sealed class AppConfig
{
    public string DbConnectionString { get; }
    public string JwtSecret { get; }
    public string Env { get; }
    public int JwtExpiryInMinutes { get; }

    public AppConfig()
    {
        DbConnectionString = GetRequired("DB_CONNECTION_STRING");
        JwtSecret = GetRequired("JWT_SECRET");

        Env = System.Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
            ?? "Development";

        JwtExpiryInMinutes = GetOptionalInt("JWT_EXPIRY_MINUTES", 60);
    }

    private static string GetRequired(string key)
    {
        var value = System.Environment.GetEnvironmentVariable(key);

        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException(
                $"Missing required environment variable: {key}");

        return value;
    }

    private static int GetOptionalInt(string key, int defaultValue)
    {
        var value = System.Environment.GetEnvironmentVariable(key);

        return int.TryParse(value, out var result)
            ? result
            : defaultValue;
    }
}