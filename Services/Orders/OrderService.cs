using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using TutorialProj.Common;
using TutorialProj.Dtos.Inventory;
using TutorialProj.Dtos.Orders;
using TutorialProj.Models;
using TutorialProj.Repositories.Interfaces;

namespace TutorialProj.Services.Orders;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _repository;
    private readonly IMemoryCache _cache;
    private readonly ILogger<OrderService> _logger;

    public OrderService(IOrderRepository repository, IMemoryCache cache, ILogger<OrderService> logger)
    {
        _repository = repository;
        _cache = cache;
        _logger = logger;
    }

    public async Task<IEnumerable<OrderSummaryDto>> GetAllAsync()
    {
        var cacheKey = CacheConstants.GetKey(CacheConstants.CacheKey.OrderList);
        if (_cache.TryGetValue(cacheKey, out IEnumerable<OrderSummaryDto>? cachedOrders))
        {
            return cachedOrders!;
        }

        // Use Include to eagerly load items. Since this is a list view, we could also
        // just query orders if we stored aggregates on the Order table itself,
        // but since we compute them dynamically, we need the items.
        // AsNoTracking makes it faster by not tracking the returned entities in EF Core.
        var orders = await _repository.FindAllAsync(
            include: q => q.Include(o => o.Items),
            asNoTracking: true
        );

        cachedOrders = orders.Select(o => new OrderSummaryDto
        {
            OrderId = o.OrderId,
            CustomerName = o.CustomerName,
            DatePlaced = o.DatePlaced,
            ItemCount = o.Items.Count,
            TotalQuantity = o.Items.Sum(i => i.Quantity)
        }).ToList();

        _cache.Set(cacheKey, cachedOrders, CacheConstants.GetDuration(CacheConstants.CacheTtl.List));

        return cachedOrders;
    }

    public async Task<OrderDetailDto?> GetByIdAsync(int id)
    {
        var cacheKey = CacheConstants.GetKey(CacheConstants.CacheKey.Order, id);
        if (_cache.TryGetValue(cacheKey, out OrderDetailDto? cachedOrder))
        {
            return cachedOrder;
        }

        // Custom repository method that already Includes the Items
        var order = await _repository.FindWithItemsAsync(id);

        if (order == null) return null;

        cachedOrder = MapOrderDetail(order);
        _cache.Set(cacheKey, cachedOrder, CacheConstants.GetDuration(CacheConstants.CacheTtl.Detail));

        return cachedOrder;
    }

    public async Task<OrderDetailDto> CreateAsync(CreateOrderDto dto)
    {
        var order = new Order
        {
            CustomerName = dto.CustomerName,
            DatePlaced = dto.DatePlaced
        };

        // Map and add the associated inventory items
        foreach (var itemDto in dto.Items)
        {
            order.Items.Add(new InventoryItem
            {
                Name = itemDto.Name,
                Quantity = itemDto.Quantity,
                Location = itemDto.Location
            });
        }

        await _repository.AddAsync(order);
        await _repository.SaveAsync();

        _logger.LogInformation("Order created: {OrderId} for customer {CustomerName} with {ItemCount} items", 
            order.OrderId, order.CustomerName, order.Items.Count);

        InvalidateOrderCache(order.OrderId);
        InvalidateInventoryListCache();

        return MapOrderDetail(order);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        // Load with items so we can clear the FK refs before delete. The relationship is
        // configured with DeleteBehavior.Restrict, so a bare Delete would crash on FK constraint
        // for any order that has items attached.
        var order = await _repository.FindWithItemsAsync(id);

        if (order == null) return false;

        order.Items.Clear();
        _repository.Delete(order);
        await _repository.SaveAsync();

        InvalidateOrderCache(id);
        InvalidateInventoryListCache();

        return true;
    }

    public async Task<OrderDetailDto?> UpdateAsync(int id, UpdateOrderDto dto)
    {
        var order = await _repository.FindWithItemsAsync(id);

        if (order == null) return null;

        // Update only provided fields
        if (!string.IsNullOrWhiteSpace(dto.CustomerName))
            order.CustomerName = dto.CustomerName;

        if (dto.DatePlaced.HasValue)
            order.DatePlaced = dto.DatePlaced.Value;

        // Entity is tracked from FindWithItemsAsync; SaveAsync persists property mutations.
        await _repository.SaveAsync();

        _logger.LogInformation("Order updated: {OrderId} for customer {CustomerName}", order.OrderId, order.CustomerName);

        InvalidateOrderCache(order.OrderId);
        InvalidateInventoryListCache();

        return MapOrderDetail(order);
    }

    private static OrderDetailDto MapOrderDetail(Order order) => new()
    {
        OrderId = order.OrderId,
        CustomerName = order.CustomerName,
        DatePlaced = order.DatePlaced,
        Items = order.Items.Select(i => new InventoryItemDto
        {
            ItemId = i.ItemId,
            Name = i.Name,
            Quantity = i.Quantity,
            Location = i.Location,
            OrderId = i.OrderId
        }).ToList()
    };

    private void InvalidateOrderCache(int orderId)
    {
        _cache.Remove(CacheConstants.GetKey(CacheConstants.CacheKey.OrderList));
        _cache.Remove(CacheConstants.GetKey(CacheConstants.CacheKey.Order, orderId));
    }

    private void InvalidateInventoryListCache()
    {
        _cache.Remove(CacheConstants.GetKey(CacheConstants.CacheKey.InventoryList));
    }
}
