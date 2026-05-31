using TutorialProj.Dtos.Inventory;
using TutorialProj.Dtos.Orders;
using TutorialProj.Models;
using TutorialProj.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace TutorialProj.Services.Orders;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _repository;

    public OrderService(IOrderRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<OrderSummaryDto>> GetAllAsync()
    {
        // Use Include to eagerly load items. Since this is a list view, we could also
        // just query orders if we stored aggregates on the Order table itself, 
        // but since we compute them dynamically, we need the items.
        // AsNoTracking makes it faster by not tracking the returned entities in EF Core.
        var orders = await _repository.FindAllAsync(
            include: q => q.Include(o => o.Items),
            asNoTracking: true
        );

        return orders.Select(o => new OrderSummaryDto
        {
            OrderId = o.OrderId,
            CustomerName = o.CustomerName,
            DatePlaced = o.DatePlaced,
            ItemCount = o.Items.Count,
            TotalQuantity = o.Items.Sum(i => i.Quantity)
        }).ToList();
    }

    public async Task<OrderDetailDto?> GetByIdAsync(int id)
    {
        // Custom repository method that already Includes the Items
        var order = await _repository.FindWithItemsAsync(id);

        if (order == null) return null;

        return new OrderDetailDto
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

        return new OrderDetailDto
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
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var order = await _repository.FindByIdAsync(id);

        if (order == null) return false;

        _repository.Delete(order);
        await _repository.SaveAsync();

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

        _repository.Update(order);
        await _repository.SaveAsync();

        return new OrderDetailDto
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
    }
}
