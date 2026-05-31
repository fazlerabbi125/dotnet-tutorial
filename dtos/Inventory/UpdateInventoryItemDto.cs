using System.ComponentModel.DataAnnotations;

namespace TutorialProj.Dtos.Inventory;

/// <summary>
/// DTO for updating an existing InventoryItem.
/// </summary>
public class UpdateInventoryItemDto
{
    [StringLength(150, MinimumLength = 1, ErrorMessage = "Item name must be between 1 and 150 characters.")]
    public string? Name { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Quantity cannot be negative.")]
    public int? Quantity { get; set; }

    [StringLength(100, ErrorMessage = "Location must not exceed 100 characters.")]
    public string? Location { get; set; }
}
