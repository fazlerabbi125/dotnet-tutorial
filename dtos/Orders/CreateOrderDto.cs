using System.ComponentModel.DataAnnotations;
using TutorialProj.Dtos.Inventory;

namespace TutorialProj.Dtos.Orders;

/// <summary>
/// Request DTO for creating a new Order.
/// </summary>
public class CreateOrderDto
{
    [Required(ErrorMessage = "Customer name is required")]
    [StringLength(150, MinimumLength = 2, ErrorMessage = "Customer name must be between 2 and 150 characters.")]
    public required string CustomerName { get; set; }

    [Required(ErrorMessage = "Date placed is required")]
    public DateTime DatePlaced { get; set; }

    /// <summary>
    /// Items to include in the order. At least one item is required.
    /// </summary>
    [Required(ErrorMessage = "At least one item is required")]
    [MinLength(1, ErrorMessage = "Order must contain at least one item")]
    public required List<CreateInventoryItemDto> Items { get; set; }
}
