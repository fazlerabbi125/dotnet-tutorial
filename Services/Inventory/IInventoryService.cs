using TutorialProj.Dtos.Inventory;

namespace TutorialProj.Services.Inventory;

public interface IInventoryService
{
    Task<IEnumerable<InventoryItemDto>> GetAllAsync();
    Task<InventoryItemDto?> GetByIdAsync(int id);
    Task<InventoryItemDto> CreateAsync(CreateInventoryItemDto dto);
    Task<InventoryItemDto?> UpdateAsync(int id, UpdateInventoryItemDto dto);
    Task<bool> DeleteAsync(int id);
}
