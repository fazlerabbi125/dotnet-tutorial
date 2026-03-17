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

// // In-memory storage using Dictionary for stable IDs after deletion
// var users = new Dictionary<int, User>
// {
//     { 1, new User { Name = "Alice", Email = "alice@example.com", Password = "password123", Gender = "female" } },
// };

// // Helper method to validate an object using data annotations
// List<string> ValidateInput(object input)
// {
//     var context = new ValidationContext(input);
//     var results = new List<ValidationResult>();
//     Validator.TryValidateObject(input, context, results, validateAllProperties: true);
//     return results.Select(r => r.ErrorMessage ?? "Validation error.").ToList();
// }

// // GET /users — returns paginated list of users
// app.MapGet("/users", (int? page, int? pageSize) =>
// {
//     var p = page ?? 1;
//     var size = pageSize ?? 10;

//     if (p < 1) p = 1;
//     if (size < 1) size = 1;
//     if (size > 100) size = 100;

//     var totalUsers = users.Count;
//     var pagedUsers = users
//         .OrderBy(kvp => kvp.Key)
//         .Skip((p - 1) * size)
//         .Take(size)
//         .Select(kvp => new { id = kvp.Key, user = kvp.Value })
//         .ToList();

//     return Results.Ok(new
//     {
//         page = p,
//         pageSize = size,
//         totalUsers,
//         data = pagedUsers
//     });
// });

// // POST /users — create a new user with validation (requires auth)
// app.MapPost("/users", (UserInput data) =>
// {
//     var errors = ValidateInput(data);
//     if (errors.Count > 0)
//     {
//         return Results.BadRequest(new { errors });
//     }

//     var newUser = new User
//     {
//         Name = data.Name,
//         Email = data.Email,
//         Password = data.Password,
//         Gender = data.Gender
//     };

//     int id = users.Count > 0 ? users.Keys.Max() + 1 : 1; // Get next ID safely
//     users[id] = newUser;
//     return Results.Created($"/users/{id}", new { id, newUser });
// }).AddEndpointFilter<AuthenticationFilter>();

// // GET /users/{id} — retrieve a single user by ID
// app.MapGet("/users/{id:int:min(0)}", (int id) =>
// {
//     if (!users.ContainsKey(id))
//     {
//         return Results.NotFound(new { error = $"User with id {id} not found." });
//     }
//     return Results.Ok(new { id, user = users[id] });
// });

// // PUT /users/{id} — update an existing user (requires auth)
// app.MapPut("/users/{id:int:min(0)}", (int id, UserInput data) =>
// {
//     if (!users.ContainsKey(id))
//     {
//         return Results.NotFound(new { error = $"User with id {id} not found." });
//     }

//     var errors = ValidateInput(data);
//     if (errors.Count > 0)
//     {
//         return Results.BadRequest(new { errors });
//     }

//     var user = users[id];
//     user.Name = data.Name;
//     user.Email = data.Email;
//     user.Password = data.Password;
//     user.Gender = data.Gender;
//     return Results.Ok(new { id, user });
// }).AddEndpointFilter<AuthenticationFilter>();

// // DELETE /users/{id} — delete a user by ID (requires auth)
// app.MapDelete("/users/{id:int:min(0)}", (int id) =>
// {
//     if (!users.ContainsKey(id))
//     {
//         return Results.NotFound(new { error = $"User with id {id} not found." });
//     }
//     users.Remove(id);
//     return Results.NoContent();
// }).AddEndpointFilter<AuthenticationFilter>();