using System.ComponentModel.DataAnnotations;

namespace TutorialProj.Dtos.Orders;

/// <summary>
/// DTO for updating an existing Order.
/// </summary>
public class UpdateOrderDto
{
    [StringLength(150, MinimumLength = 2, ErrorMessage = "Customer name must be between 2 and 150 characters.")]
    public string? CustomerName { get; set; }

    public DateTime? DatePlaced { get; set; }
}
