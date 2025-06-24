using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace EcommerceAPI.DTOs
{
    /// <summary>
    /// DTO for updating cart item quantity
    /// </summary>
    public class UpdateCartItemDto
    {
        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, 100, ErrorMessage = "Quantity must be between 1 and 100")]
        public int Quantity { get; set; }

    }
}