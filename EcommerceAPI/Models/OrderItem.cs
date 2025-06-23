using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace EcommerceAPI.Models
{
    public class OrderItem
    {
        public int Id { get; set; }

        //Quantity of this prodcut in order
        public int Quantity { get; set; }        // CRITICAL: Price at the time of purchase(not current product price)
        // This ensures historical accuracy of orders even if product prices change
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Price { get; set; }

        //Link to the product
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        //Link to parent order
        public int OrderId { get; set; }
        public Order Order { get; set; } = null!;



    }
}