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



        }


    }
}