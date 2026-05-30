using TutorialProj.Dtos.Inventory;

namespace TutorialProj.Dtos.Orders;

/// <summary>
/// Full detail view of an Order, including its related InventoryItems.
/// </summary>
public class OrderDetailDto
{
    public int OrderId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public DateTime DatePlaced { get; set; }
    public List<InventoryItemDto> Items { get; set; } = new();
}
