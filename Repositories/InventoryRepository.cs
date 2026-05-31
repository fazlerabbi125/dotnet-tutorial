using TutorialProj.Models;
using TutorialProj.Repositories.Interfaces;

namespace TutorialProj.Repositories;

public class InventoryRepository : Repository<InventoryItem>, IInventoryRepository
{
    public InventoryRepository(AppDbContext context) : base(context)
    {
    }
}
