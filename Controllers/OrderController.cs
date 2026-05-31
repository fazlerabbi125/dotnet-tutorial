using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TutorialProj.Dtos.Orders;

namespace TutorialProj.Services.Orders;

[ApiController]
[Route("api/orders")]
public class OrderController : ControllerBase
{
    private readonly IOrderService _service;

    public OrderController(IOrderService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<OrderSummaryDto>>> GetAll()
    {
        var orders = await _service.GetAllAsync();
        return Ok(orders);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<OrderDetailDto>> GetById(int id)
    {
        var order = await _service.GetByIdAsync(id);

        if (order == null)
        {
            return NotFound(new { error = $"Order with ID {id} not found." });
        }

        return Ok(order);
    }

    [HttpPost]
    // Ensures only authenticated users with a valid JWT token can access this endpoint
    [Authorize]
    public async Task<ActionResult<OrderDetailDto>> Create([FromBody] CreateOrderDto dto)
    {
        var createdOrder = await _service.CreateAsync(dto);

        return CreatedAtAction(nameof(GetById), new { id = createdOrder.OrderId }, createdOrder);
    }

    [HttpDelete("{id}")]
    // Ensures only authenticated users with the "Manager" role can access this endpoint
    [Authorize(Roles = TutorialProj.Constants.AppRoles.Manager)]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound(new { error = $"Order with ID {id} not found." });
        }

        return NoContent();
    }

    [HttpPut("{id}")]
    // Ensures only authenticated users with the "Manager" role can access this endpoint
    [Authorize(Roles = TutorialProj.Constants.AppRoles.Manager)]
    public async Task<ActionResult<OrderDetailDto>> Update(int id, [FromBody] UpdateOrderDto dto)
    {
        var updatedOrder = await _service.UpdateAsync(id, dto);

        if (updatedOrder == null)
        {
            return NotFound(new { error = $"Order with ID {id} not found." });
        }

        return Ok(updatedOrder);
    }
}
