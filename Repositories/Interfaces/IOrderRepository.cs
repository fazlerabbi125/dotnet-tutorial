using TutorialProj.Models;

namespace TutorialProj.Repositories.Interfaces;

public interface IOrderRepository : IRepository<Order>
{
    Task<Order?> FindWithItemsAsync(int orderId);
}
