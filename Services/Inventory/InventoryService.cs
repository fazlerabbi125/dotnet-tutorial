using Microsoft.Extensions.Caching.Memory;
using TutorialProj.Dtos.Inventory;
using TutorialProj.Models;
using TutorialProj.Repositories.Interfaces;

namespace TutorialProj.Services.Inventory;

public class InventoryService : IInventoryService
{
    private readonly IInventoryRepository _repository;
    private readonly IMemoryCache _cache;
    private const string CacheKey = "inventory_list";

    // Dependency Injection (DI) passes the repository and cache to the service.
    // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection
    public InventoryService(IInventoryRepository repository, IMemoryCache cache)
    {
        _repository = repository;
        _cache = cache;
    }

    public async Task<IEnumerable<InventoryItemDto>> GetAllAsync()
    {
        // 1. Try to get the list from the in-memory cache first
        if (!_cache.TryGetValue(CacheKey, out IEnumerable<InventoryItemDto>? cachedList))
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

            // 3. Store the result in cache for 30 seconds
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromSeconds(30));
            
            _cache.Set(CacheKey, cachedList, cacheOptions);
        }

        return cachedList!;
    }

    public async Task<InventoryItemDto?> GetByIdAsync(int id)
    {
        var item = await _repository.FindByIdAsync(id);
        
        if (item == null) return null;

        return new InventoryItemDto
        {
            ItemId = item.ItemId,
            Name = item.Name,
            Quantity = item.Quantity,
            Location = item.Location,
            OrderId = item.OrderId
        };
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

        // Invalidate cache since we added a new item
        _cache.Remove(CacheKey);

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

        // Invalidate cache
        _cache.Remove(CacheKey);
        
        return true;
    }
}
