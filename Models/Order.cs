using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace EcommerceAPI.Models
{
    public class Order
    {
        public int Id { get; set; }

        //Link to user who placed order
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;

        //When order is placed
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        //Total amount paid (calculated from OrderItems)
        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalAmount { get; set; }

        //Shipping Information
        [MaxLength(500)]
        public string ShippingAddress { get; set; } = string.Empty;

        //Order Status for tracking fulfillment
        [MaxLength(50)]
        public string OrderStatus { get; set; } = "Pending";        //Stripe payment session ID for tracking
        [MaxLength(200)]
        public string? StripeSessionId { get; set; }

        //All items in order
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

        // Additional Stripe properties
        public string? PaymentIntentId { get; set; }
        public string? CustomerEmail { get; set; }
        public string? BillingAddress { get; set; }
        public string Status { get; set; } = "Pending";

    }
}