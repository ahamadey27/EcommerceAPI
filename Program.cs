using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using EcommerceAPI.Data;
using EcommerceAPI.Models;

var builder = WebApplication.CreateBuilder(args);

// Configure for Railway deployment
builder.WebHost.ConfigureKestrel(options =>
{
    var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
    options.ListenAnyIP(int.Parse(port));
});

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddRazorPages(); // Add Razor Pages support
builder.Services.AddEndpointsApiExplorer();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .AllowAnyOrigin() // Allow all origins for Railway deployment
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// Add Swagger with JWT authentication support
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "EcommerceAPI", Version = "v1" });
    
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// Register ApplicationDbContext with SQLite
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure ASP.NET Core Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.User.RequireUniqueEmail = true;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders()
.AddRoles<IdentityRole>();

// Configure Identity cookie options (do NOT add AddAuthentication/AddCookie again)
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.None;
    options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
    options.Cookie.HttpOnly = true;
    options.LoginPath = "/Login";
    options.LogoutPath = "/Logout";
    options.AccessDeniedPath = "/Login";
    options.SlidingExpiration = true;
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
});

// Configure JWT Authentication - with better error handling
try 
{
    var jwtSecret = builder.Configuration["JWT:Secret"] ?? "ThisIsMySecretKeyForJWTTokenGenerationAndItShouldBeAtLeast32Characters";
    var jwtIssuer = builder.Configuration["JWT:Issuer"] ?? "EcommerceAPI";
    var jwtAudience = builder.Configuration["JWT:Audience"] ?? "EcommerceAPI-Users";

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ClockSkew = TimeSpan.Zero
        };
    });
}
catch (Exception ex)
{
    Console.WriteLine($"JWT Configuration Error: {ex.Message}");
}

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseSwagger();
app.UseSwaggerUI();

// Don't force HTTPS redirect in production (Azure handles this)
if (!app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}

app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.UseStaticFiles(); // Serve static files
app.MapControllers();
app.MapRazorPages(); // Map Razor Pages

// Health check endpoints  
app.MapGet("/api", () => Results.Json(new { 
    Message = "E-Commerce API is running!", 
    Documentation = "/swagger",
    Health = "/health",
    Environment = app.Environment.EnvironmentName,
    Timestamp = DateTime.UtcNow
}));

app.MapGet("/health", () => Results.Json(new { 
    Status = "Healthy", 
    Timestamp = DateTime.UtcNow,
    Environment = app.Environment.EnvironmentName 
}));

// Initialize database with error handling
_ = Task.Run(async () =>
{
    try
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        
        await context.Database.EnsureCreatedAsync();
        await DbInitializer.SeedAsync(userManager, roleManager);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Database initialization error: {ex.Message}");
    }
});

app.Run();