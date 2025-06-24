using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EcommerceAPI.DTOs
{
    /// <summary>
    /// DTO for returning checkout session status details
    /// </summary>
    public class CheckoutSessionDetailsDto
    {
        public string SessionId { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;
        public string? PaymentIntentId { get; set; }
        public long? AmountTotal { get; set; }
        public string Currency { get; set; } = string.Empty;
        public string? CustomerEmail { get; set; }

    }
}