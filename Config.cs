public sealed class AppConfig
{
    public string DbConnectionString { get; }
    public string JwtSecret { get; }
    public string Env { get; }
    public int JwtExpiryInMinutes { get; }
    public string? AdminEmail { get; }
    public string? AdminPassword { get; }

    public AppConfig()
    {
        DbConnectionString = GetRequired("DB_CONNECTION_STRING");
        JwtSecret = GetRequired("JWT_SECRET");

        // HMAC-SHA256 requires a key of at least 256 bits (32 ASCII characters).
        if (JwtSecret.Length < 32)
            throw new InvalidOperationException(
                "JWT_SECRET must be at least 32 characters (256 bits) for HMAC-SHA256.");

        Env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
            ?? "Development";

        JwtExpiryInMinutes = GetOptionalInt("JWT_EXPIRY_MINUTES", 60);
        AdminEmail = GetOptional("ADMIN_EMAIL");
        AdminPassword = GetOptional("ADMIN_PASSWORD");
    }

    private static string GetRequired(string key)
    {
        var value = Environment.GetEnvironmentVariable(key);

        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException(
                $"Missing required environment variable: {key}");

        return value;
    }

    private static string? GetOptional(string key)
    {
        return Environment.GetEnvironmentVariable(key);
    }

    private static int GetOptionalInt(string key, int defaultValue)
    {
        var value = Environment.GetEnvironmentVariable(key);

        return int.TryParse(value, out var result)
            ? result
            : defaultValue;
    }
}