using System;
using System.ComponentModel.DataAnnotations;

namespace EcommerceAPI.Models
{
    public class CartItem
    {
        public int Id { get; set; }
        
        [Required]
        public int CartId { get; set; }  // Foreign key to ShoppingCart
        
        [Required]
        public int ProductId { get; set; }  // Foreign key to Product
        
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public ShoppingCart Cart { get; set; } = null!;
        public Product Product { get; set; } = null!;
    }
}