using TutorialProj.Models;

namespace TutorialProj.Repositories.Interfaces;

public interface IInventoryRepository : IRepository<InventoryItem>
{
    // Add domain-specific methods here if needed, for example:
    // Task<IEnumerable<InventoryItem>> FindByLocationAsync(string location);
}
