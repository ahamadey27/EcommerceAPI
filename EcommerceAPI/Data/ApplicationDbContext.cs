using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using EcommerceAPI.Models;

namespace EcommerceAPI.Data
{
    // Inherits from IdentityDbContext to get all ASP.NET Core Identity tables
    // while adding our custom business entities
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        // Constructor accepts DbContextOptions for configuration (connection string, etc.)
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSet properties expose your entities as queryable collections
        // Entity Framework uses these to generate database tables        public DbSet<Product> Products { get; set; }
        public DbSet<ShoppingCart> ShoppingCarts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }


        // OnModelCreating is where you'll configure entity relationships using Fluent API
        // We'll configure relationships in Step 1.4
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder); // CRITICAL: Configures Identity tables
                                           // Entity relationship configurations using Fluent API to define foreign keys, constraints, etc.

            // Configure ApplicationUser -> ShoppingCart (One-to-One)
            builder.Entity<ApplicationUser>()
                .HasOne(u => u.ShoppingCart) //User has one cart
                .WithOne(c => c.User)        //Cart belongs to user
                .HasForeignKey<ShoppingCart>(c => c.UserId) //Foreign key is in shopping cart
                .OnDelete(DeleteBehavior.Cascade); //Delete cart when user is deleted 

            // Configure ApplicationUser -> Order (One-to-Many)
            builder.Entity<ApplicationUser>()
                .HasMany(u => u.Orders)                // User has many orders
                .WithOne(o => o.User)                  // Order belongs to one user
                .HasForeignKey(o => o.UserId)          // Foreign key is UserId in Order table
                .OnDelete(DeleteBehavior.Restrict);    // Prevent user deletion if orders exist

            // Configure ShoppingCart -> CartItem (One-to-Many)
            builder.Entity<ShoppingCart>()
                .HasMany(c => c.Items)                 // Cart has many items
                .WithOne(ci => ci.ShoppingCart)        // CartItem belongs to one cart
                .HasForeignKey(ci => ci.ShoppingCartId) // Foreign key in CartItem table
                .OnDelete(DeleteBehavior.Cascade);     // Delete items when cart is deleted

            //Configure Product -> CartItem (One-to-Many)
            builder.Entity<Product>()
                .HasMany(p => p.CartItems)          // User has many orders
                .WithOne(ci => ci.Product)          // Order belongs to one user
                .HasForeignKey(ci => ci.ProductId)  // Foreign key is UserId in Order table
                .OnDelete(DeleteBehavior.Cascade);  // Prevent user deletion if orders exist

            // Configure ShoppingCart -> CartItem (One-to-Many)
            builder.Entity<ShoppingCart>()
                .HasMany(c => c.Items)                 // Cart has many items
                .WithOne(ci => ci.ShoppingCart)        // CartItem belongs to one cart
                .HasForeignKey(ci => ci.ShoppingCartId) // Foreign key in CartItem table
                .OnDelete(DeleteBehavior.Cascade);     // Delete items when cart is deleted

            // Configure Product -> CartItem (One-to-Many)
            builder.Entity<Product>()
                .HasMany(p => p.CartItems)             // Product can be in many carts
                .WithOne(ci => ci.Product)             // CartItem references one product
                .HasForeignKey(ci => ci.ProductId)     // Foreign key in CartItem table
                .OnDelete(DeleteBehavior.Cascade);     // Remove from carts if product deleted

            // Configure Order -> OrderItem (One-to-Many)
            builder.Entity<Order>()
                .HasMany(o => o.OrderItems)            // Order has many items
                .WithOne(oi => oi.Order)               // OrderItem belongs to one order
                .HasForeignKey(oi => oi.OrderId)       // Foreign key in OrderItem table
                .OnDelete(DeleteBehavior.Cascade);     // Delete items when order is deleted

            // Configure Product -> OrderItem (One-to-Many)
            builder.Entity<Product>()
                .HasMany(p => p.OrderItems)            // Product can be in many orders
                .WithOne(oi => oi.Product)             // OrderItem references one product
                .HasForeignKey(oi => oi.ProductId)     // Foreign key in OrderItem table
                .OnDelete(DeleteBehavior.Restrict);    // Keep order history even if product deleted


        }


    }
}