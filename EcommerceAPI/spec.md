# Project: Full-Stack E-Commerce Platform API

**Goal:** To architect and build a modern, full-featured e-commerce API using ASP.NET Core and C#. The system will feature a logic-heavy, multi-layered design with a complex data model to handle products, user-specific shopping carts, secure JWT-based authentication, role-based authorization, and real-world payment processing via Stripe integration. The final API will be deployed to Microsoft Azure.

---

# Components

## Environment/Hosting
- **Local Development Machine:** Windows/macOS/Linux
- **IDE:** Visual Studio Code with C# Extension
- **Version Control:** Git
- **Cloud Hosting:** Microsoft Azure (App Service and SQL Database Free Tiers)

## Software Components

### Web Application Backend
- **Framework:** ASP.NET Core Web API (using Controllers)
- **Language:** C#
- **Database ORM:** Entity Framework Core
- **Authentication:** ASP.NET Core Identity with JSON Web Tokens (JWT)
- **API Documentation:** Swashbuckle (Swagger UI)

### Core Logic Services & Data Layer
- **Data Context:** `ApplicationDbContext.cs` (Inherits from `IdentityDbContext` to unify business and identity schemas)
- **Controllers:**
    - `AuthController.cs`: Handles user registration and JWT generation.
    - `ProductsController.cs`: Manages CRUD operations for products (Admin) and public viewing.
    - `ShoppingCartController.cs`: Manages user-specific shopping cart operations.
    - `CheckoutController.cs`: Initiates the Stripe payment process.
    - `WebhookController.cs`: Handles asynchronous order fulfillment from Stripe events.

### External APIs
- **Payment Processing:** Stripe API (for handling payments and checkouts).

---

# Core Services and Data Structures

## `ApplicationDbContext.cs` (Data Context)
- **Responsibilities:**
    - Bridges C# entity models with the SQL database.
    - Manages schema for both custom entities (`Product`, `Order`) and ASP.NET Core Identity (`ApplicationUser`).
    - Configures entity relationships and constraints using the Fluent API.
- **Key Data Models:**
    - `ApplicationUser`: Extends `IdentityUser` with custom fields.
    - `Product`: Represents an item for sale.
    - `ShoppingCart` & `CartItem`: Models a user's cart and the items within it.
    - `Order` & `OrderItem`: Represents a completed purchase, capturing price at the time of sale.

## `AuthController.cs` (Controller)
- **Responsibilities:**
    - Provides endpoints for user registration (`/register`) and login (`/login`).
    - Validates user credentials against the database using `UserManager`.
    - Generates and signs a stateless JWT upon successful login, including user ID and roles as claims.
- **Key Methods (Conceptual):**
    - `Register(RegisterDto)`: Creates a new `ApplicationUser`.
    - `Login(LoginDto)`: Authenticates a user and returns a JWT.

## Configuration (`appsettings.json` / Azure App Settings)
- **DefaultConnection:** Connection string for the SQL database (local or Azure SQL).
- **JWT:Secret:** The secret key for signing and validating JSON Web Tokens.
- **JWT:Issuer & JWT:Audience:** Identifiers for the token issuer and intended audience.
- **Stripe:PublishableKey & Stripe:SecretKey:** API keys for Stripe integration.
- **Stripe:WebhookSecret:** The secret for verifying the authenticity of incoming Stripe webhooks.

---

# Development Plan

## Phase 1: Foundation and Data Modeling
- [x] **Step 1.1: Project Initialization**
    - [x] Create new ASP.NET Core Web API project (with Controllers).
    - [x] Enable OpenAPI (Swagger) support.
    - [x] Install NuGet packages: `Microsoft.EntityFrameworkCore.SqlServer`, `Microsoft.EntityFrameworkCore.Tools`.
- [x] **Step 1.2: Architect Data Schema**
    - [x] Define entity classes: `ApplicationUser`, `Product`, `ShoppingCart`, `CartItem`, `Order`, `OrderItem`.
- [x] **Step 1.3: Configure DbContext**
    - [x] Create `ApplicationDbContext` inheriting from `IdentityDbContext<ApplicationUser>`.
    - [x] Add `DbSet<>` properties for all custom entities.
    - [x] Register the `DbContext` in `Program.cs` and configure the connection string.
- [x] **Step 1.4: Define Entity Relationships**
    - [x] Use the Fluent API in `OnModelCreating` to configure one-to-many and one-to-one relationships.
- [x] **Step 1.5: Create Database with Migrations**
    - [x] Run `add-migration InitialCreate`.
    - [x] Run `update-database` to generate the SQL schema.

## Phase 2: Secure Authentication and Authorization
- [x] **Step 2.1: Integrate ASP.NET Core Identity**
    - [x] Install NuGet package: `Microsoft.AspNetCore.Identity.EntityFrameworkCore`.
    - [x] Configure Identity services in `Program.cs` using `AddIdentity`.
- [x] **Step 2.2: Implement JWT-Based Authentication**
    - [x] Install NuGet package: `Microsoft.AspNetCore.Authentication.JwtBearer`.
    - [x] Configure JWT secret and issuer in `appsettings.json`.
    - [x] Configure JWT Bearer authentication middleware in `Program.cs`.
    - [x] Create `AuthController.cs` with custom `Register` and `Login` endpoints that generate JWTs.
- [x] **Step 2.3: Implement and Seed User Roles**
    - [x] Enable role management via `AddRoles<IdentityRole>()`.
    - [x] Create a `DbInitializer` class to seed "Admin" and "User" roles and create a default admin user on application startup.
- [x] **Step 2.4: Secure Endpoints**
    - [x] Apply `[Authorize]` and `[Authorize(Roles = "Admin")]` attributes to controllers and actions.

## Phase 3: Core E-Commerce API Endpoints
- [x] **Step 3.1: Build Product Management Controller (`ProductsController.cs`)**
    - [x] Implement secure CRUD endpoints (POST, GET, PUT, DELETE) for products, restricted to "Admin" role.
    - [x] Use DTOs to prevent over-posting.
- [x] **Step 3.2: Implement Public Catalog Endpoints**
    - [x] Add `[AllowAnonymous]` endpoints to `ProductsController` for public product viewing and searching.
- [x] **Step 3.3: Build Shopping Cart Controller (`ShoppingCartController.cs`)**
    - [x] Secure the controller with `[Authorize]`.
    - [x] Implement logic to get, add to, and remove from the cart of the currently authenticated user (identified via JWT claims).
    - [x] Implement on-the-fly cart creation for new users.

## Phase 4: Payment Processing and Order Fulfillment
- [x] **Step 4.1: Integrate Stripe Payment Gateway**
    - [x] Install NuGet package: `Stripe.net`.
    - [x] Configure Stripe API keys in `appsettings.json` and initialize in `Program.cs`.
- [x] **Step 4.2: Implement Stripe Checkout Flow (`CheckoutController.cs`)**
    - [x] Create an endpoint to generate a Stripe `SessionCreateOptions` object from the user's cart.
    - [x] Set the `ClientReferenceId` to the application's `UserId` to link the transaction.
    - [x] Return the Stripe Checkout Session URL to the client.
- [x] **Step 4.3: Implement Order Fulfillment via Webhooks (`WebhookController.cs`)**
    - [x] Create a public endpoint to receive webhook events from Stripe.
    - [x] Implement webhook signature verification using the `WebhookSecret` for security.
    - [x] Handle the `checkout.session.completed` event to create a new `Order`, clear the user's `ShoppingCart`, and save to the database.

## Phase 5: Deployment, Testing, and Frontend Interaction
- [x] **Step 5.0: Local API Testing and Stripe Integration**
    - [x] Test all API endpoints using REST Client or Swagger UI.
    - [x] Test authentication (admin and customer accounts).
    - [x] Test product management (create, view products).
    - [x] Test shopping cart functionality (add, view, update items).
    - [x] Set up Stripe test keys and verify checkout session creation.
    - [x] Use ngrok to expose local webhook endpoint for Stripe testing.
    - [x] Test complete payment flow: register → login → add products → checkout → webhook.
- [ ] **Step 5.1: Provision and Configure Azure Resources**
    - [ ] Create a free-tier Azure App Service.
    - [ ] Create a free-tier Azure SQL Database.
    - [ ] Configure the database firewall to allow access from the App Service.
- [ ] **Step 5.2: Publish API to Azure**
    - [ ] Use the Azure CLI or VS Code Azure Extensions to deploy.
    - [ ] Configure the Azure SQL connection string in Azure App Service Configuration.
    - [ ] Run database migrations against the Azure SQL database using dotnet CLI or Azure Cloud Shell.
- [ ] **Step 5.3: Build Minimalist Frontend**
    - [ ] Create a separate ASP.NET Core Razor Pages project.
    - [ ] Use JavaScript `fetch` API to call the deployed API endpoints.
    - [ ] Implement logic to store and send the JWT in the `Authorization` header for protected requests.
- [ ] **Step 5.4: End-to-End Testing**
    - [ ] Use Postman, Thunder Client (VS Code extension), or the built-in REST Client to test every API endpoint directly.
    - [ ] Perform a full user flow test on the live frontend: register, log in, add to cart, initiate payment.
    - [ ] Verify order creation in the Azure SQL database after a successful test payment.

## Phase 6: (Optional) Advanced Enhancements & Ongoing Maintenance
- [ ] **Step 6.1: Implement JWT Refresh Tokens**
    - [ ] Add a mechanism to issue and use refresh tokens for extended user sessions without re-authentication.
- [ ] **Step 6.2: Implement Advanced Search and Pagination**
    - [ ] Enhance product search with filtering by category, price range, and add pagination to results.
- [ ] **Step 6.3: Containerize the Application**
    - [ ] Create a `Dockerfile` for the API.
    - [ ] Deploy the container to Azure Container Apps or another container service.
- [ ] **Step 6.4: Implement Application Monitoring & Logging**
    - [ ] Integrate a robust logging framework (e.g., Serilog).
    - [ ] Set up Azure Application Insights for monitoring performance and errors.