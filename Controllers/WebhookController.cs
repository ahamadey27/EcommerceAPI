using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using EcommerceAPI.Data;
using EcommerceAPI.Models;

namespace EcommerceAPI.Controllers
{
    /// <summary>
    /// Controller for handling simplified order processing
    /// No longer uses Stripe - just processes demo orders
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class WebhookController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<WebhookController> _logger;

        public WebhookController(
            ApplicationDbContext context, 
            ILogger<WebhookController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Test endpoint for order completion (simplified demo)
        /// POST: api/webhook/test
        /// </summary>
        [HttpPost("test")]
        public async Task<IActionResult> TestWebhook([FromBody] JsonElement payload)
        {
            try
            {
                _logger.LogInformation("Test webhook received");
                
                // Extract data from the test payload
                var eventType = payload.GetProperty("type").GetString();
                var sessionData = payload.GetProperty("data").GetProperty("object");
                
                if (eventType == "checkout.session.completed")
                {
                    var sessionId = sessionData.GetProperty("id").GetString() ?? "test_session";
                    var clientReferenceId = sessionData.GetProperty("client_reference_id").GetString();
                    
                    _logger.LogInformation($"Processing test checkout completion for user: {clientReferenceId}");
                    
                    if (string.IsNullOrEmpty(clientReferenceId))
                    {
                        _logger.LogWarning("No client reference ID found in test webhook");
                        return BadRequest("No client reference ID");
                    }

                    // Find the user's cart
                    var cart = await _context.ShoppingCarts
                        .Include(c => c.CartItems)
                        .ThenInclude(ci => ci.Product)
                        .FirstOrDefaultAsync(c => c.UserId == clientReferenceId);

                    if (cart == null || !cart.CartItems.Any())
                    {
                        _logger.LogWarning($"No cart found for user {clientReferenceId}");
                        return BadRequest("Cart not found");
                    }

                    // Create order
                    var order = new Order
                    {
                        UserId = clientReferenceId,
                        StripeSessionId = sessionId,
                        PaymentIntentId = $"pi_test_{Guid.NewGuid().ToString("N")[..24]}",
                        TotalAmount = cart.CartItems.Sum(ci => ci.Product.Price * ci.Quantity),
                        Status = "Completed",
                        OrderDate = DateTime.UtcNow,
                        ShippingAddress = "Demo Address - 123 Main St, City, State 12345",
                        OrderStatus = "Completed",
                        OrderItems = cart.CartItems.Select(ci => new OrderItem
                        {
                            ProductId = ci.ProductId,
                            Price = ci.Product.Price,
                            Quantity = ci.Quantity
                        }).ToList()
                    };

                    _context.Orders.Add(order);

                    // Update inventory
                    foreach (var cartItem in cart.CartItems)
                    {
                        var product = cartItem.Product;
                        product.StockQuantity -= cartItem.Quantity;
                        _context.Products.Update(product);
                    }

                    // Clear the cart
                    _context.CartItems.RemoveRange(cart.CartItems);

                    await _context.SaveChangesAsync();

                    _logger.LogInformation($"Test order created successfully: {order.Id}");
                    return Ok(new { message = "Test webhook processed successfully", orderId = order.Id });
                }

                return Ok(new { message = "Test event type not handled" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing test webhook");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}