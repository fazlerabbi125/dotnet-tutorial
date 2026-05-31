using System.ComponentModel.DataAnnotations;

namespace TutorialProj.Dtos.Inventory;

/// <summary>
/// DTO for creating a new InventoryItem.
/// DTOs (Data Transfer Objects) are used to decouple the API shape from your internal database models.
/// https://learn.microsoft.com/en-us/aspnet/core/tutorials/first-web-api?view=aspnetcore-10.0#prevent-over-posting
/// </summary>
public class CreateInventoryItemDto
{
    [Required]
    [StringLength(150, MinimumLength = 1, ErrorMessage = "Item name must be between 1 and 150 characters.")]
    public required string Name { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Quantity cannot be negative.")]
    public int Quantity { get; set; } = 0;

    [StringLength(100, ErrorMessage = "Location must not exceed 100 characters.")]
    public string? Location { get; set; }
}
