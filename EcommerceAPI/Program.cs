var builder = WebApplication.CreateBuilder(args);

// Add basic services for testing
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

// Simple test endpoints
app.MapGet("/", () => "E-Commerce API is running! Visit /swagger for documentation");
app.MapGet("/health", () => new { Status = "Healthy", Time = DateTime.UtcNow });

app.Run();