using System.ComponentModel.DataAnnotations;
using TutorialProj.Dtos.Inventory;

namespace TutorialProj.Dtos.Orders;

/// <summary>
/// Request DTO for creating a new Order.
/// </summary>
public class CreateOrderDto
{
    [Required]
    [StringLength(150, MinimumLength = 2, ErrorMessage = "Customer name must be between 2 and 150 characters.")]
    public required string CustomerName { get; set; }

    [Required]
    public DateTime DatePlaced { get; set; }

    // Allows creating an order along with its items in a single request.
    public List<CreateInventoryItemDto> Items { get; set; } = new();
}
