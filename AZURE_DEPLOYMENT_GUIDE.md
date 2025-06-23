# Azure Deployment Guide - E-Commerce Platform

This guide will walk you through deploying your ASP.NET Core e-commerce application to Azure using free tier resources.

## Prerequisites

1. **Azure Account**: [Sign up for free](https://azure.microsoft.com/free/) if you don't have one
2. **Azure CLI**: [Download and install](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli)
3. **Git**: Ensure your project is in a Git repository
4. **VS Code** with Azure Extensions (optional but recommended)

## Phase 1: Prepare Your Application for Deployment

### Step 1.1: Update Connection Strings
Update your `appsettings.json` to support Azure SQL:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=EcommerceApiDb;Trusted_Connection=true;MultipleActiveResultSets=true",
    "AzureConnection": "Server=tcp:{your-server}.database.windows.net,1433;Initial Catalog={your-database};Persist Security Info=False;User ID={your-username};Password={your-password};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
  }
}
```

### Step 1.2: Update Program.cs for Azure
Add environment-specific connection string handling:

```csharp
// In Program.cs, update the connection string configuration:
var connectionString = builder.Environment.IsProduction() 
    ? builder.Configuration.GetConnectionString("AzureConnection")
    : builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
```

### Step 1.3: Prepare Frontend for Azure
Update the API base URL in your frontend's `auth.js`:

```javascript
// Update API_BASE_URL to use environment detection
const API_BASE_URL = window.location.hostname === 'localhost' 
    ? 'http://localhost:5000/api' 
    : 'https://your-api-app-name.azurewebsites.net/api';
```

## Phase 2: Deploy the Backend API

### Step 2.1: Login to Azure CLI
```bash
az login
```

### Step 2.2: Create Resource Group
```bash
az group create --name rg-ecommerce --location "East US"
```

### Step 2.3: Create Azure SQL Server and Database (Free Tier)
```bash
# Create SQL Server
az sql server create \
    --name sql-ecommerce-server \
    --resource-group rg-ecommerce \
    --location "East US" \
    --admin-user sqladmin \
    --admin-password "YourStrongPassword123!"

# Create SQL Database (Basic tier - cheapest)
az sql db create \
    --resource-group rg-ecommerce \
    --server sql-ecommerce-server \
    --name EcommerceApiDb \
    --service-objective Basic

# Configure firewall to allow Azure services
az sql server firewall-rule create \
    --resource-group rg-ecommerce \
    --server sql-ecommerce-server \
    --name AllowAzureServices \
    --start-ip-address 0.0.0.0 \
    --end-ip-address 0.0.0.0
```

### Step 2.4: Create App Service Plan (Free Tier)
```bash
az appservice plan create \
    --name plan-ecommerce \
    --resource-group rg-ecommerce \
    --sku F1 \
    --is-linux
```

### Step 2.5: Create Web App for API
```bash
az webapp create \
    --resource-group rg-ecommerce \
    --plan plan-ecommerce \
    --name your-ecommerce-api \
    --runtime "DOTNET|8.0"
```

### Step 2.6: Configure App Settings
```bash
# Set connection string
az webapp config connection-string set \
    --resource-group rg-ecommerce \
    --name your-ecommerce-api \
    --connection-string-type SQLAzure \
    --settings DefaultConnection="Server=tcp:sql-ecommerce-server.database.windows.net,1433;Initial Catalog=EcommerceApiDb;Persist Security Info=False;User ID=sqladmin;Password=YourStrongPassword123!;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"

# Set other app settings
az webapp config appsettings set \
    --resource-group rg-ecommerce \
    --name your-ecommerce-api \
    --settings JWT__Secret="ThisIsMySecretKeyForJWTTokenGenerationAndItShouldBeAtLeast32Characters" \
               JWT__Issuer="EcommerceAPI" \
               JWT__Audience="EcommerceAPI-Users" \
               JWT__ExpiryInMinutes="60"
```

### Step 2.7: Deploy API Code
```bash
# Navigate to your API project directory
cd c:\Users\hamad\Documents\GitHub\EcommerceAPI\EcommerceAPI

# Deploy using Azure CLI
az webapp deployment source config-zip \
    --resource-group rg-ecommerce \
    --name your-ecommerce-api \
    --src publish.zip
```

**Alternative: Deploy via VS Code**
1. Install "Azure App Service" extension in VS Code
2. Right-click on your API project
3. Select "Deploy to Web App..."
4. Follow the prompts

### Step 2.8: Run Database Migrations
```bash
# Install Entity Framework tools if not already installed
dotnet tool install --global dotnet-ef

# Update connection string in appsettings.json temporarily for migration
# Then run migrations
dotnet ef database update --connection "Server=tcp:sql-ecommerce-server.database.windows.net,1433;Initial Catalog=EcommerceApiDb;Persist Security Info=False;User ID=sqladmin;Password=YourStrongPassword123!;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
```

## Phase 3: Deploy the Frontend

### Step 3.1: Create Web App for Frontend
```bash
az webapp create \
    --resource-group rg-ecommerce \
    --plan plan-ecommerce \
    --name your-ecommerce-frontend \
    --runtime "DOTNET|8.0"
```

### Step 3.2: Update Frontend API URL
Before deploying, update your frontend's `auth.js`:

```javascript
const API_BASE_URL = 'https://your-ecommerce-api.azurewebsites.net/api';
```

### Step 3.3: Deploy Frontend Code
```bash
# Navigate to your frontend project directory
cd c:\Users\hamad\Documents\GitHub\EcommerceAPI\EcommerceFrontend

# Deploy using Azure CLI
az webapp deployment source config-zip \
    --resource-group rg-ecommerce \
    --name your-ecommerce-frontend \
    --src publish.zip
```

## Phase 4: Configure CORS for Production

### Step 4.1: Update API CORS Settings
In your API's `Program.cs`, update CORS to allow your frontend domain:

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
            "http://localhost:5045", // Local development
            "https://your-ecommerce-frontend.azurewebsites.net" // Production
        )
        .AllowAnyHeader()
        .AllowAnyMethod();
    });
});
```

## Phase 5: Test Your Deployed Application

### Step 5.1: Access Your Applications
- **API**: https://your-ecommerce-api.azurewebsites.net
- **Frontend**: https://your-ecommerce-frontend.azurewebsites.net

### Step 5.2: Test Key Functionality
1. Register a new user
2. Login
3. Browse products
4. Add items to cart
5. Complete checkout process

## Phase 6: Monitor and Maintain

### Step 6.1: View Logs
```bash
# Stream logs from API
az webapp log tail --resource-group rg-ecommerce --name your-ecommerce-api

# Stream logs from Frontend
az webapp log tail --resource-group rg-ecommerce --name your-ecommerce-frontend
```

### Step 6.2: Scale Up If Needed
```bash
# Upgrade to Basic tier if you need more resources (costs money)
az appservice plan update \
    --name plan-ecommerce \
    --resource-group rg-ecommerce \
    --sku B1
```

## Cost Estimation (Free Tier)

- **App Service Plan (F1)**: FREE
- **SQL Database (Basic)**: ~$5/month
- **Two Web Apps**: FREE (on F1 plan)
- **Total**: ~$5/month

## Troubleshooting Common Issues

### Issue 1: Database Connection Errors
- Verify connection string is correct
- Check firewall rules on SQL Server
- Ensure database migrations have been run

### Issue 2: CORS Errors
- Verify CORS policy includes your frontend domain
- Check that both HTTP and HTTPS are configured if needed

### Issue 3: Application Not Starting
- Check application logs: `az webapp log tail`
- Verify all required app settings are configured
- Ensure .NET runtime version matches your application

### Issue 4: 500 Internal Server Errors
- Check detailed error logs in Azure portal
- Verify all dependencies are included in deployment
- Check environment-specific configuration

## Security Considerations for Production

1. **Use Azure Key Vault** for storing sensitive configuration
2. **Enable HTTPS only** on both web apps
3. **Configure custom domains** with SSL certificates
4. **Implement proper logging** and monitoring
5. **Set up backup strategies** for your database

## Next Steps

1. Set up CI/CD pipeline using GitHub Actions
2. Implement Application Insights for monitoring
3. Configure custom domains
4. Set up automated backups
5. Implement caching strategies

---

*Note: Replace `your-ecommerce-api` and `your-ecommerce-frontend` with your actual app names throughout this guide.*
