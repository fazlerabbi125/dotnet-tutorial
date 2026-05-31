namespace TutorialProj.Constants;

/// <summary>
/// Cache key definitions and time-to-live (TTL) values for in-memory caching.
/// </summary>
public static class CacheKeys
{
    /// <summary>
    /// Cache keys for different data types.
    /// </summary>
    public enum CacheKeyType
    {
        /// <summary>Cache key for inventory items list</summary>
        InventoryList = 1,

        /// <summary>Cache key for order summaries list</summary>
        OrderList = 2,

        /// <summary>Cache key for individual inventory item</summary>
        InventoryItem = 3,

        /// <summary>Cache key for individual order with items</summary>
        Order = 4
    }

    /// <summary>
    /// Converts cache key enum to string representation.
    /// </summary>
    public static string GetKeyString(CacheKeyType keyType) => keyType switch
    {
        CacheKeyType.InventoryList => "inventory_list",
        CacheKeyType.OrderList => "order_list",
        CacheKeyType.InventoryItem => "inventory_item_{0}",
        CacheKeyType.Order => "order_{0}",
        _ => throw new ArgumentException($"Unknown cache key type: {keyType}")
    };

    /// <summary>
    /// Returns the cache key string for a specific inventory item.
    /// </summary>
    public static string GetInventoryItemKey(int itemId) =>
        string.Format(GetKeyString(CacheKeyType.InventoryItem), itemId);

    /// <summary>
    /// Returns the cache key string for a specific order.
    /// </summary>
    public static string GetOrderKey(int orderId) =>
        string.Format(GetKeyString(CacheKeyType.Order), orderId);
}

/// <summary>
/// Cache time-to-live (TTL) settings.
/// </summary>
public static class CacheTtl
{
    /// <summary>TTL for inventory list cache (30 seconds)</summary>
    public static readonly TimeSpan InventoryListTtl = TimeSpan.FromSeconds(30);

    /// <summary>TTL for order list cache (30 seconds)</summary>
    public static readonly TimeSpan OrderListTtl = TimeSpan.FromSeconds(30);

    /// <summary>TTL for individual inventory item cache (60 seconds)</summary>
    public static readonly TimeSpan InventoryItemTtl = TimeSpan.FromSeconds(60);

    /// <summary>TTL for individual order cache (60 seconds)</summary>
    public static readonly TimeSpan OrderTtl = TimeSpan.FromSeconds(60);
}
