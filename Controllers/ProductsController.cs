using Microsoft.AspNetCore.Mvc;
// using System.Collections.Generic; // System available by default in latest version

[ApiController]

[Route("api/[controller]")] // means the route is based on the controller name (ProductsController → products)

public class ProductsController : ControllerBase

{

    [HttpGet] // maps a method to an HTTP GET request at /api/products

    public ActionResult<List<string>> Get()

    {

        return new List<string> { "Apple", "Banana", "Orange" };

    }

    [HttpGet("featured")] // custom route at /api/products/featured
    public string GetFeaturedProduct() => "Mango";

    [HttpPost] // POST

    public ActionResult<string> Post([FromBody] string newProduct)

    {

        return $"Added: {newProduct}";

    }

    [HttpPut("{id}")] // PUT

    public ActionResult<string> Put(int id, [FromBody] string updatedProduct)

    {

        return $"Updated product {id} to: {updatedProduct}";

    }

    [HttpDelete("{id}")] // DELETE

    public ActionResult<string> Delete(int id)

    {

        return $"Deleted product with ID: {id}";

    }

}

