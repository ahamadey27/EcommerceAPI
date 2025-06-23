# Ecommerce Frontend

This is the minimalist frontend for the Ecommerce API, built with ASP.NET Core Razor Pages.

## Features

- **User Authentication**: Registration and login with JWT tokens
- **Product Catalog**: Browse products (public access)
- **Shopping Cart**: Add, update, and remove items from cart
- **Checkout**: Stripe integration for secure payments
- **Admin Features**: Admin users can add new products

## Running the Frontend

1. Make sure the API is running at `https://localhost:7155`
2. Navigate to the frontend directory:
   ```bash
   cd EcommerceFrontend
   ```
3. Run the frontend:
   ```bash
   dotnet run
   ```
4. Open your browser to `https://localhost:7045`

## Configuration

The frontend is configured to connect to the API at `https://localhost:7155`. This can be changed in `appsettings.json`:

```json
{
  "ApiSettings": {
    "BaseUrl": "https://localhost:7155"
  }
}
```

## Pages

- **Home** (`/`): Welcome page with navigation
- **Register** (`/Register`): User registration
- **Login** (`/Login`): User authentication
- **Products** (`/Products`): Product catalog and admin product management
- **Cart** (`/Cart`): Shopping cart management
- **Success** (`/Success`): Successful payment confirmation

## Authentication

The frontend uses JWT tokens stored in localStorage for authentication. The JavaScript utilities handle:

- Token storage and retrieval
- Adding Authorization headers to API calls
- Navigation updates based on authentication status
- Cart count updates

## Testing the Full Flow

1. **Register** a new account or **Login** with existing credentials
2. **Browse Products** - view the product catalog
3. **Add Items** to your cart
4. **View Cart** and proceed to checkout
5. **Complete Payment** through Stripe (use test card: 4242 4242 4242 4242)
6. **Confirmation** on the success page

## Admin Features

Users with the "Admin" role can:
- View the "Add Product" button on the Products page
- Create new products through the modal form

## Tech Stack

- **Frontend**: ASP.NET Core Razor Pages
- **Styling**: Bootstrap 5
- **JavaScript**: Vanilla JS with Fetch API
- **Authentication**: JWT tokens
- **Payment**: Stripe Checkout
