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
        }        // DbSet properties expose your entities as queryable collections
        // Entity Framework uses these to generate database tables
        public DbSet<Product> Products { get; set; }
        public DbSet<ShoppingCart> ShoppingCarts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }        // OnModelCreating is where you'll configure entity relationships using Fluent API
        // We'll configure relationships in Step 1.4
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder); // CRITICAL: Configures Identity tables

            // Configure ApplicationUser -> ShoppingCart (One-to-One)
            builder.Entity<ApplicationUser>()
                .HasOne(u => u.ShoppingCart)
                .WithOne(c => c.User)
                .HasForeignKey<ShoppingCart>(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

        // Configure ApplicationUser -> Order (One-to-Many)
        builder.Entity<ApplicationUser>()
                .HasMany(u => u.Orders)
                .WithOne(o => o.User)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);        // Configure ShoppingCart -> CartItem (One-to-Many)
        builder.Entity<ShoppingCart>()
                .HasMany(c => c.CartItems)
                .WithOne(ci => ci.Cart)
                .HasForeignKey(ci => ci.CartId)
                .OnDelete(DeleteBehavior.Cascade);

        // Configure Product -> CartItem (One-to-Many)
        builder.Entity<Product>()
                .HasMany(p => p.CartItems)
                .WithOne(ci => ci.Product)
                .HasForeignKey(ci => ci.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

        // Configure Order -> OrderItem (One-to-Many)
        builder.Entity<Order>()
                .HasMany(o => o.OrderItems)
                .WithOne(oi => oi.Order)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

        // Configure Product -> OrderItem (One-to-Many)
        builder.Entity<Product>()
                .HasMany(p => p.OrderItems)
                .WithOne(oi => oi.Product)
                .HasForeignKey(oi => oi.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

        // Configure decimal precision for monetary values
        builder.Entity<Product>()
                .Property(p => p.Price)
                .HasPrecision(18, 2);

        builder.Entity<Order>()
                .Property(o => o.TotalAmount)
                .HasPrecision(18, 2);

        builder.Entity<OrderItem>()
                .Property(oi => oi.Price)
                .HasPrecision(18, 2);

        // Configure indexes for better query performance
        builder.Entity<Product>()
                .HasIndex(p => p.Name);

        builder.Entity<Order>()
                .HasIndex(o => o.OrderDate);        builder.Entity<CartItem>()
                .HasIndex(ci => new { ci.CartId, ci.ProductId })
                .IsUnique();
            
        }


    }
}