using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EcommerceAPI.DTOs
{
    /// <summary>
    /// DTO for returning individual cart item information
    /// </summary>
    public class CartItemResponseDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal ProductPrice { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; } // Quantity * ProductPrice
        public string? ProductImageUrl { get; set; }
        public string ProductCategory { get; set; } = string.Empty;
    }
}