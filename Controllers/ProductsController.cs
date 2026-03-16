using Microsoft.AspNetCore.Mvc;
// using System.Collections.Generic; // System available by default in latest version

[ApiController]

[Route("api/[controller]")] // means the route is based on the controller name (ProductsController → products)

public class ProductsController : ControllerBase

{
    private static readonly List<string> _products = new List<string> { "Apple", "Banana", "Orange" };

    [HttpGet] // maps a method to an HTTP GET request at /api/products

    public ActionResult<List<string>> Get()

    {

        return Ok(_products);
    }

    [HttpPost] // POST

    public ActionResult<string> Post([FromBody] string newProduct)

    {
        _products.Add(newProduct); // Add new product to the list
        return $"Added: {newProduct}";

    }

    [HttpPut("{id:int:min(0)}")] // PUT

    public ActionResult<string> Put(int id, [FromBody] string updatedProduct)

    {
        if (id < 0 || id >= _products.Count)

        {

            return NotFound($"Product with ID {id} not found.");

        }
        _products[id] = updatedProduct;
        return $"Updated product {id} to: {updatedProduct}";

    }

    [HttpDelete("{id:int:min(0)}")] // DELETE

    public ActionResult<string> Delete(int id)

    {
        if (id < 0 || id >= _products.Count)

        {

            return NotFound($"Product with ID {id} not found.");

        }
        _products.RemoveAt(id);
        return NoContent();

    }

}

