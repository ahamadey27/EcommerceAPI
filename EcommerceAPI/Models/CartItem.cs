using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EcommerceAPI.Models
{
    public class CartItem
    {
        public int Id { get; set; }

        // Foreign key to the shopping cart
        public int CartId { get; set; }
        public ShoppingCart Cart { get; set; } = null!;

        // Foreign key to the product
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        // How many of this product the user wants
        public int Quantity { get; set; }

        // When this item is added to cart
        public DateTime CreatedAt { get; set; }

        // When this item is last updated in the cart
        public DateTime UpdatedAt { get; set; }
        public object ShoppingCart { get; internal set; }
    }
}