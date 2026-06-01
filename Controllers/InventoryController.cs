using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TutorialProj.Common;
using TutorialProj.Dtos.Inventory;

namespace TutorialProj.Services.Inventory;

// ApiController enables automatic model validation (HTTP 400 if CreateInventoryItemDto rules fail).
// https://learn.microsoft.com/en-us/aspnet/core/web-api/#apicontroller-attribute
[ApiController]
[Route("api/inventory")] // Base route for all actions in this controller
public class InventoryController : ControllerBase
{
    private readonly IInventoryService _service;

    public InventoryController(IInventoryService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<InventoryItemDto>>> GetAll()
    {
        var items = await _service.GetAllAsync();
        return Ok(items);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<InventoryItemDto>> GetById(int id)
    {
        var item = await _service.GetByIdAsync(id);

        if (item == null)
        {
            return NotFound(new { error = $"Inventory item with ID {id} not found." });
        }

        return Ok(item);
    }

    [HttpPost]
    // Ensures only authenticated users with a valid JWT token can access this endpoint
    [Authorize]
    public async Task<ActionResult<InventoryItemDto>> Create([FromBody] CreateInventoryItemDto dto)
    {
        var createdItem = await _service.CreateAsync(dto);

        // CreatedAtAction returns a 201 status code and a Location header pointing to the GetById action
        return CreatedAtAction(nameof(GetById), new { id = createdItem.ItemId }, createdItem);
    }

    [HttpDelete("{id}")]
    // Ensures only authenticated users with the "admin" role can access this endpoint
    [Authorize(Roles = UserRoles.Admin)]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound(new { error = $"Inventory item with ID {id} not found." });
        }

        return NoContent(); // 204 No Content is standard for a successful DELETE
    }

    [HttpPut("{id}")]
    // Ensures only authenticated users with the "admin" role can access this endpoint
    [Authorize(Roles = UserRoles.Admin)]
    public async Task<ActionResult<InventoryItemDto>> Update(int id, [FromBody] UpdateInventoryItemDto dto)
    {
        var updatedItem = await _service.UpdateAsync(id, dto);

        if (updatedItem == null)
        {
            return NotFound(new { error = $"Inventory item with ID {id} not found." });
        }

        return Ok(updatedItem);
    }
}
