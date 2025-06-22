using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EcommerceAPI.Models
{
    public class ShoppingCart
    {
        public int Id { get; set; }

        // Foreign key to link cart to specific user
        public string UserId { get; set; } = string.Empty;

        // Navigation property - EF Core uses this to create the relationship
        public ApplicationUser User { get; set; } = null!;

        // Collection of items in this cart
        public ICollection<CartItem> Items { get; set; } = new List<CartItem>();

        // Timestamps for tracking cart activity
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;



    }
}