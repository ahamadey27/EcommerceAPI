using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using EcommerceAPI.Data;
using EcommerceAPI.DTOs;
using EcommerceAPI.Models;

namespace EcommerceAPI.Controllers
{
    /// <summary>
    /// Controller for handling simplified checkout process
    /// Creates orders directly from shopping cart without external payment processing
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Only authenticated users can checkout
    public class CheckoutController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CheckoutController> _logger;

        public CheckoutController(ApplicationDbContext context, ILogger<CheckoutController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Create order directly from user's cart (simplified demo checkout)
        /// POST: api/checkout/create-order
        /// </summary>
        [HttpPost("create-order")]
        public async Task<ActionResult<CheckoutSessionResponseDto>> CreateOrder()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized("User not found in token.");

            // Get user's cart with items
            var cart = await _context.ShoppingCarts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null || !cart.CartItems.Any())
                return BadRequest("Shopping cart is empty.");

            // Calculate total
            var totalAmount = cart.CartItems.Sum(ci => ci.Product.Price * ci.Quantity);

            // Create order
            var order = new Order
            {
                UserId = userId,
                OrderDate = DateTime.UtcNow,
                TotalAmount = totalAmount,
                ShippingAddress = "Demo Address - 123 Main St, City, State 12345",
                OrderStatus = "Pending",
                Status = "Completed", // For demo purposes
                CustomerEmail = User.FindFirst(ClaimTypes.Email)?.Value
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Create order items
            var orderItems = cart.CartItems.Select(ci => new OrderItem
            {
                OrderId = order.Id,
                ProductId = ci.ProductId,
                Quantity = ci.Quantity,
                Price = ci.Product.Price
            }).ToList();

            _context.OrderItems.AddRange(orderItems);

            // Clear the cart
            _context.CartItems.RemoveRange(cart.CartItems);
            
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Order {order.Id} created successfully for user {userId}");

            return Ok(new CheckoutSessionResponseDto
            {
                SessionId = order.Id.ToString(),
                CheckoutUrl = $"/success?orderId={order.Id}",
                PublishableKey = "demo-checkout-completed"
            });
        }

        /// <summary>
        /// Get order details
        /// GET: api/checkout/order/{orderId}
        /// </summary>
        [HttpGet("order/{orderId}")]
        public async Task<ActionResult<CheckoutSessionDetailsDto>> GetOrderDetails(int orderId)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized();

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

            if (order == null)
                return NotFound("Order not found.");

            return Ok(new CheckoutSessionDetailsDto
            {
                SessionId = order.Id.ToString(),
                PaymentStatus = "succeeded",
                PaymentIntentId = $"demo_pi_{order.Id}",
                AmountTotal = (long)(order.TotalAmount * 100), // Convert to cents for consistency
                Currency = "usd",
                CustomerEmail = order.CustomerEmail
            });
        }

        /// <summary>
        /// Get checkout information and available options
        /// GET: api/checkout/info
        /// </summary>
        [HttpGet("info")]
        public IActionResult GetCheckoutInfo()
        {
            return Ok(new { 
                message = "Simplified Demo Checkout API ready", 
                availableEndpoints = new[] { 
                    "POST /api/checkout/create-order - Create order directly from cart",
                    "GET /api/checkout/order/{orderId} - Get order details" 
                }
            });
        }

        private string? GetCurrentUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
    }
}
