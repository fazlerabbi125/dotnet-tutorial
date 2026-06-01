using Microsoft.Extensions.Caching.Memory;
using TutorialProj.Common;
using TutorialProj.Dtos.Inventory;
using TutorialProj.Models;
using TutorialProj.Repositories.Interfaces;

namespace TutorialProj.Services.Inventory;

public class InventoryService : IInventoryService
{
    private readonly IInventoryRepository _repository;
    private readonly IMemoryCache _cache;
    private readonly ILogger<InventoryService> _logger;

    // Dependency Injection (DI) passes the repository, cache, and logger to the service.
    // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection
    public InventoryService(IInventoryRepository repository, IMemoryCache cache, ILogger<InventoryService> logger)
    {
        _repository = repository;
        _cache = cache;
        _logger = logger;
    }

    public async Task<IEnumerable<InventoryItemDto>> GetAllAsync()
    {
        // 1. Try to get the list from the in-memory cache first
        var cacheKey = CacheConstants.GetKey(CacheConstants.CacheKey.InventoryList);
        if (!_cache.TryGetValue(cacheKey, out IEnumerable<InventoryItemDto>? cachedList))
        {
            // 2. If not in cache, query the database using the repository
            // AsNoTracking is used for performance since we only read the data.
            var items = await _repository.FindAllAsync(asNoTracking: true);

            // Map entities to DTOs
            cachedList = items.Select(i => new InventoryItemDto
            {
                ItemId = i.ItemId,
                Name = i.Name,
                Quantity = i.Quantity,
                Location = i.Location,
                OrderId = i.OrderId
            }).ToList();

            // 3. Store the result in cache with TTL
            _cache.Set(cacheKey, cachedList, CacheConstants.GetDuration(CacheConstants.CacheTtl.List));
        }

        return cachedList!;
    }

    public async Task<InventoryItemDto?> GetByIdAsync(int id)
    {
        var cacheKey = CacheConstants.GetKey(CacheConstants.CacheKey.InventoryItem, id);
        if (_cache.TryGetValue(cacheKey, out InventoryItemDto? cachedItem))
        {
            return cachedItem;
        }

        var item = await _repository.FindByIdAsync(id);

        if (item == null) return null;

        var itemDto = new InventoryItemDto
        {
            ItemId = item.ItemId,
            Name = item.Name,
            Quantity = item.Quantity,
            Location = item.Location,
            OrderId = item.OrderId
        };

        _cache.Set(cacheKey, itemDto, CacheConstants.GetDuration(CacheConstants.CacheTtl.Detail));

        return itemDto;
    }

    public async Task<InventoryItemDto> CreateAsync(CreateInventoryItemDto dto)
    {
        var entity = new InventoryItem
        {
            Name = dto.Name,
            Quantity = dto.Quantity,
            Location = dto.Location
        };

        await _repository.AddAsync(entity);
        await _repository.SaveAsync();

        _logger.LogInformation("Inventory item created: {ItemId} - {Name} (Qty: {Quantity})", entity.ItemId, entity.Name, entity.Quantity);

        InvalidateInventoryCache(entity.ItemId);

        return new InventoryItemDto
        {
            ItemId = entity.ItemId,
            Name = entity.Name,
            Quantity = entity.Quantity,
            Location = entity.Location,
            OrderId = entity.OrderId
        };
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _repository.FindByIdAsync(id);

        if (entity == null) return false;

        _repository.Delete(entity);
        await _repository.SaveAsync();

        InvalidateInventoryCache(entity.ItemId);

        return true;
    }

    public async Task<InventoryItemDto?> UpdateAsync(int id, UpdateInventoryItemDto dto)
    {
        var entity = await _repository.FindByIdAsync(id);

        if (entity == null) return null;

        // Update only provided fields
        if (!string.IsNullOrWhiteSpace(dto.Name))
            entity.Name = dto.Name;

        if (dto.Quantity.HasValue)
            entity.Quantity = dto.Quantity.Value;

        if (dto.Location != null)
            entity.Location = dto.Location;

        // Entity is tracked from FindByIdAsync; SaveAsync persists property mutations.
        await _repository.SaveAsync();

        _logger.LogInformation("Inventory item updated: {ItemId} - {Name} (Qty: {Quantity})", entity.ItemId, entity.Name, entity.Quantity);

        InvalidateInventoryCache(entity.ItemId);

        return new InventoryItemDto
        {
            ItemId = entity.ItemId,
            Name = entity.Name,
            Quantity = entity.Quantity,
            Location = entity.Location,
            OrderId = entity.OrderId
        };
    }

    private void InvalidateInventoryCache(int itemId)
    {
        // Invalidate both the list cache and the specific item cache
        _cache.Remove(CacheConstants.GetKey(CacheConstants.CacheKey.InventoryList));
        _cache.Remove(CacheConstants.GetKey(CacheConstants.CacheKey.InventoryItem, itemId));
        _logger.LogDebug("Inventory cache invalidated for item {ItemId}", itemId);
    }
}
