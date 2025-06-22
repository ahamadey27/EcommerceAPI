using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EcommerceAPI.Models
{
    public class CartItem
    {
        public int Id { get; set; }

        //How man of this product the user wants
        public int Quantity { get; set; }

        //Foreign ket to the product
        public int ProductId { get; set; }

        public Product Product { get; set; } = null!;

        // Foreign key to the shopping cart
        public int ShoppingCartId { get; set; }
        public ShoppingCart ShoppingCart { get; set; } = null!;

        //when this item is added to cart

        public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    }
}