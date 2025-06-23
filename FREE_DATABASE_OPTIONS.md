# Free Database Deployment Options

## Option 1: SQLite Database (Recommended for Portfolio/Demo)

### Benefits:
- **Completely FREE**
- **No external dependencies**
- **Works perfectly with Azure App Service**
- **Great for portfolio projects**
- **Easy deployment**

### Setup Steps:

1. **Install SQLite NuGet Package:**
```bash
dotnet add package Microsoft.EntityFrameworkCore.Sqlite
```

2. **Update Program.cs:**
```csharp
// Replace SQL Server with SQLite
var connectionString = builder.Environment.IsProduction() 
    ? "Data Source=ecommerce.db"  // SQLite file in Azure
    : builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));  // Change to UseSqlite
```

3. **Update appsettings.json:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=ecommerce.db"
  }
}
```

4. **Deploy to Azure:**
- SQLite database file will be created automatically
- No additional Azure resources needed
- Database persists with your app

### Limitations:
- Single file database (not ideal for high-traffic production)
- No advanced SQL Server features
- Perfect for demos and portfolio projects

---

## Option 2: Azure Database for PostgreSQL (Flexible Server)

### Benefits:
- **FREE tier available** (Burstable B1ms with 1 vCore, 2GB RAM)
- **20GB storage included**
- **Full PostgreSQL features**
- **More "production-like" than SQLite**

### Setup Steps:

1. **Install PostgreSQL NuGet Package:**
```bash
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
```

2. **Create Free PostgreSQL Server:**
```bash
az postgres flexible-server create \
    --resource-group rg-ecommerce \
    --name postgres-ecommerce \
    --location "East US" \
    --admin-user postgres \
    --admin-password "YourStrongPassword123!" \
    --sku-name Standard_B1ms \
    --tier Burstable \
    --version 13 \
    --storage-size 32
```

3. **Update Program.cs:**
```csharp
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));
```

---

## Option 3: PlanetScale (MySQL) - Free Tier

### Benefits:
- **FREE tier with 5GB storage**
- **Cloud-native MySQL**
- **Excellent for learning**
- **Easy to upgrade**

### Setup:
1. Sign up at planetscale.com
2. Create free database
3. Get connection string
4. Use MySQL Entity Framework provider

---

## Option 4: Supabase (PostgreSQL) - Free Tier

### Benefits:
- **FREE tier with 500MB storage**
- **Full PostgreSQL**
- **Real-time features included**
- **Good for modern apps**

---

## Recommendation: Go with SQLite for Your Portfolio

For your e-commerce portfolio project, I recommend **SQLite** because:

✅ **Completely free**
✅ **Zero configuration**
✅ **Works perfectly on Azure**
✅ **No monthly costs**
✅ **Perfect for demonstrations**
✅ **Easy to backup/restore**

The application will work exactly the same - users can register, login, add to cart, checkout, etc. The only difference is the database engine running behind the scenes.

Would you like me to help you convert your project to use SQLite for free Azure deployment?
