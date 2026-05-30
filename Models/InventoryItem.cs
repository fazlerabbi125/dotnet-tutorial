/*
Provides attribute classes that are used to define metadata for enforcing data validation rules, dictate display formatting, and define database mapping constraints directly within your class properties. These are enforced in in .NET, not in database.
*/
using System.ComponentModel.DataAnnotations;

// https://learn.microsoft.com/en-us/aspnet/core/data/ef-mvc/complex-data-model
// https://learn.microsoft.com/en-us/ef/core/modeling/relationships/conventions

namespace TutorialProj.Models;

public class InventoryItem : TimeStampMixin
{
    /*
    Any property named Id or <EntityName>Id (like ItemId)
    is automatically treated as the primary key
    */
    //[Key] // Optional if primary key property is named Id or <EntityName>Id, but can be used for clarity or if you want to use a different naming convention.
    public int ItemId { get; set; }

    // [Required] // Not mandatory as it can be implied by the reference type of the property
    [StringLength(150, MinimumLength = 1, ErrorMessage = "Item name must be between 1 and 150 characters.")]
    public required string Name { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Quantity cannot be negative.")]
    public int Quantity { get; set; } = 0;

    [StringLength(100, MinimumLength = 1, ErrorMessage = "Location must be between 1 and 100 characters.")]
    public string? Location { get; set; }

    public int? OrderId { get; set; } // Foreign key to Order, nullable to allow items not associated with an order

    // [ForeignKey("OrderId")] // Explicitly specify the foreign key relationship. Not needed if name of the foreign key property is <NavigationPropertyName>Id (like OrderId) which EF Core can conventionally recognize.
    public Order? Order { get; set; }

    public string DisplayInfo()
    {
        return $"Item: {Name} | Quantity: {Quantity}" + (string.IsNullOrEmpty(Location) ? "" : $" | Location: {Location}");
    }
}
