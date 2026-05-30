namespace TutorialProj.Dtos.Inventory;

/// <summary>
/// DTO for returning an InventoryItem to the client.
/// We intentionally exclude the 'Order' property here to prevent infinite circular JSON reference errors.
/// </summary>
public class InventoryItemDto
{
    public int ItemId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string? Location { get; set; }
    public int? OrderId { get; set; }
}
