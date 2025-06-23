using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace EcommerceAPI.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty;

        // Use decimal for currency to avoid floating-point precision issues
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        // Track inventory levels
        public int StockQuantity { get; set; }

        //URL to product image(could be Azure Blob Storage URL later)
        [MaxLength(500)]
        public string? ImageUrl { get; set; }

        //Timestamp for auditing 
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties - products can be in many cart items and order items
        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

        // Product category for organization and filtering
        [MaxLength(100)]
        public string Category { get; set; } = string.Empty;
    }

    
}
