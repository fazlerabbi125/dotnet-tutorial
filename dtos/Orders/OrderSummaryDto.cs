namespace TutorialProj.Dtos.Orders;

/// <summary>
/// Lightweight read-only projection of an Order used in list responses.
/// Avoids serialising the full Items collection on every list call (performance optimisation).
/// </summary>
public class OrderSummaryDto
{
    public int OrderId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public DateTime DatePlaced { get; set; }

    /// <summary>Number of distinct inventory items in the order.</summary>
    public int ItemCount { get; set; }

    /// <summary>Sum of all item quantities in the order.</summary>
    public int TotalQuantity { get; set; }
}
