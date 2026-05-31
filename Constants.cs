namespace TutorialProj.Constants;

/// <summary>
/// Shared application constants used across authentication, authorization, and seeding.
/// </summary>
public static class AppRoles
{
    public const string Manager = "Manager";
    public const string User = "User";

    public static readonly string[] All = [Manager, User];
}

public static class AppCommands
{
    public const string CreateAdmin = "create-admin";
}


public static class CacheConstants
{
    public enum CacheKey
    {
        InventoryList,
        InventoryItem,
        OrderList,
        Order
    }

    public enum CacheTtl
    {
        List,
        Detail
    }

    public static string GetKey(CacheKey key) => key switch
    {
        CacheKey.InventoryList => "inventory:list",
        CacheKey.OrderList => "orders:list",
        _ => throw new ArgumentException($"Cache key '{key}' requires an identifier.")
    };

    public static string GetKey(CacheKey key, int id) => key switch
    {
        CacheKey.InventoryItem => $"inventory:item:{id}",
        CacheKey.Order => $"orders:item:{id}",
        _ => GetKey(key)
    };

    public static TimeSpan GetDuration(CacheTtl ttl) => ttl switch
    {
        CacheTtl.List => TimeSpan.FromSeconds(30),
        CacheTtl.Detail => TimeSpan.FromSeconds(60),
        _ => throw new ArgumentOutOfRangeException(nameof(ttl), ttl, "Unknown cache TTL.")
    };
}
