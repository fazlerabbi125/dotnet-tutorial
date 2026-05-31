using TutorialProj.Dtos.Orders;

namespace TutorialProj.Services.Orders;

public interface IOrderService
{
    Task<IEnumerable<OrderSummaryDto>> GetAllAsync();
    Task<OrderDetailDto?> GetByIdAsync(int id);
    Task<OrderDetailDto> CreateAsync(CreateOrderDto dto);
    Task<bool> DeleteAsync(int id);
}
