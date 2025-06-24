using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EcommerceAPI.DTOs
{
    public class CheckoutSessionResponseDto
    {
        public string SessionId { get; set; } = string.Empty;
        public string CheckoutUrl { get; set; } = string.Empty;
        public string PublishableKey { get; set; } = string.Empty;

    }
}