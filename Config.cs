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

        Env = System.Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
            ?? "Development";

        JwtExpiryInMinutes = GetOptionalInt("JWT_EXPIRY_MINUTES", 60);
        AdminEmail = GetOptional("ADMIN_EMAIL");
        AdminPassword = GetOptional("ADMIN_PASSWORD");
    }

    private static string GetRequired(string key)
    {
        var value = System.Environment.GetEnvironmentVariable(key);

        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException(
                $"Missing required environment variable: {key}");

        return value;
    }

    private static string? GetOptional(string key)
    {
        return System.Environment.GetEnvironmentVariable(key);
    }

    private static int GetOptionalInt(string key, int defaultValue)
    {
        var value = System.Environment.GetEnvironmentVariable(key);

        return int.TryParse(value, out var result)
            ? result
            : defaultValue;
    }
}