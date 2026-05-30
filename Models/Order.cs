using System.ComponentModel.DataAnnotations;

namespace TutorialProj.Models;

public class Order: TimeStampMixin
{
    public int OrderId { get; set; }

    [StringLength(150, MinimumLength = 2, ErrorMessage = "Customer name must be between 2 and 150 characters.")]
    public required string CustomerName { get; set; }

    [DataType(DataType.Date)] // Necessary when value must be formatted as a date or time particularly
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}")]
    public required DateTime DatePlaced { get; set; }

    public ICollection<InventoryItem> Items { get; set; } = new HashSet<InventoryItem>();//ICollection<T> or a type such as List<T> or HashSet<T>. If you specify ICollection<T>, EF creates a HashSet<T> collection by default.

    public void AddItem(InventoryItem item)
    {
        ArgumentNullException.ThrowIfNull(item, nameof(item));

        // Ensure consistency of relationship
        item.Order = this;
        item.OrderId = this.OrderId;

        Items.Add(item);
    }

    public bool RemoveItem(int itemId)
    {
        var item = Items.FirstOrDefault(i => i.ItemId == itemId);

        if (item == null)
            return false;

        Items.Remove(item);

        // break relationship
        item.Order = null;
        item.OrderId = null;

        return true;
    }

    public string GetOrderSummary()
    {
        var totalItems = Items.Count;
        var totalQuantity = Items.Sum(i => i.Quantity);

        return
            $"Order #{OrderId} | Customer: {CustomerName} | " +
            $"Items: {totalItems} | Total Quantity: {totalQuantity} | " +
            $"Date: {DatePlaced:yyyy-MM-dd}";
    }
}
