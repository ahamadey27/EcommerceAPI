var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers(); // Enable controller-based routing
builder.Services.AddOpenApi(); // Keep OpenAPI (Swagger) support

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi(); // Enable OpenAPI specification
}

app.UseHttpsRedirection();

// Map controller routes instead of minimal API endpoints
app.MapControllers();

app.Run();