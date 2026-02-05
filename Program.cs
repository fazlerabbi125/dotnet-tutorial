using Models.Item;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi(); // Document name is v1
builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi(); // https://localhost:{port}/openapi/v1.json
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "API V1");
    });
}

// Configure the HTTP request pipeline.

// Remove HTTPS redirection so you can test with http

// app.UseHttpsRedirection();

// app.UseAuthorization();

var items = new List<Item>{
    new Item { Name = "Item 1", Description = "Description for Item 1" },
};

app.MapControllers();




app.MapGet("/", () => "Welcome to the Simple Web API!");

app.MapGet("/items", () => items);

app.MapPost("/items", (Item data) =>
{
    items.Add(data);
    return Results.Created($"New item created with id {items.Count - 1}", data);
});


app.MapGet("/items/{id:int:min(0)}", (int id) =>
{
    if (id < 0 || id >= items.Count)
    {
        return Results.NotFound();
    }
    var item = items[id];
    return Results.Ok(item);
});

app.MapPut("/items/{id:int:min(0)}", (int id, Item data) =>
{
    if (id < 0 || id >= items.Count)
    {
        return Results.NotFound();
    }
    var item = items[id];
    item.Name = data.Name;
    item.Description = data.Description;
    return Results.Ok(item);
});

app.MapDelete("/items/{id:int:min(0)}", (int id) =>
{
    if (id < 0 || id >= items.Count)
    {
        return Results.NotFound();
    }
    items.RemoveAt(id);
    return Results.NoContent();
});

app.Run();