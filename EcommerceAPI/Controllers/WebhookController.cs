using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Stripe;
using Stripe.Checkout;
using EcommerceAPI.Data;
using EcommerceAPI.Models;

namespace EcommerceAPI.Controllers
{
    /// <summary>
    /// Controller for handling Stripe webhook events
    /// Processes payment completions and creates orders
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class WebhookController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<WebhookController> _logger;

        public WebhookController(
            ApplicationDbContext context, 
            IConfiguration configuration,
            ILogger<WebhookController> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Handle Stripe webhook events
        /// POST: api/webhook/stripe
        /// </summary>
        [HttpPost("stripe")]
        public async Task<IActionResult> HandleStripeWebhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            var stripeSignature = Request.Headers["Stripe-Signature"];

            try
            {
                // Verify webhook signature for security
                var webhookSecret = _configuration["Stripe:WebhookSecret"];
                var stripeEvent = EventUtility.ConstructEvent(
                    json, 
                    stripeSignature, 
                    webhookSecret
                );

                _logger.LogInformation($"Received Stripe webhook: {stripeEvent.Type}");                // Handle different event types
                switch (stripeEvent.Type)
                {
                    case "checkout.session.completed":
                        await HandleCheckoutSessionCompleted(stripeEvent);
                        break;
                    
                    case "payment_intent.succeeded":
                        _logger.LogInformation("Payment succeeded");
                        break;
                    
                    case "payment_intent.payment_failed":
                        await HandlePaymentFailed(stripeEvent);
                        break;
                    
                    default:
                        _logger.LogInformation($"Unhandled event type: {stripeEvent.Type}");
                        break;
                }

                return Ok();
            }
            catch (StripeException ex)
            {
                _logger.LogError($"Stripe webhook error: {ex.Message}");
                return BadRequest($"Webhook signature verification failed: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Webhook processing error: {ex.Message}");
                return StatusCode(500, "Internal server error processing webhook");
            }
        }

        /// <summary>
        /// Handle successful checkout completion
        /// Creates order and clears cart
        /// </summary>
        private async Task HandleCheckoutSessionCompleted(Event stripeEvent)
        {
            var session = stripeEvent.Data.Object as Session;
            if (session == null)
            {
                _logger.LogError("Invalid checkout session in webhook");
                return;
            }

            var userId = session.ClientReferenceId;
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogError("No user ID found in checkout session");
                return;
            }

            _logger.LogInformation($"Processing completed checkout for user: {userId}");

            try
            {
                // Get user's cart with items
                var cart = await _context.ShoppingCarts
                    .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if (cart == null || !cart.CartItems.Any())
                {
                    _logger.LogWarning($"No cart found for user {userId}");
                    return;
                }                // Create order from cart
                var order = new Order
                {
                    UserId = userId,
                    OrderDate = DateTime.UtcNow,
                    TotalAmount = cart.CartItems.Sum(ci => ci.Product.Price * ci.Quantity),
                    Status = "Completed",
                    StripeSessionId = session.Id,
                    PaymentIntentId = session.PaymentIntentId,
                    CustomerEmail = session.CustomerEmail,
                    ShippingAddress = FormatAddress(session.CustomerDetails?.Address) ?? string.Empty,
                    BillingAddress = FormatAddress(session.CustomerDetails?.Address) ?? string.Empty
                };

                // Create order items from cart items
                order.OrderItems = cart.CartItems.Select(ci => new OrderItem
                {
                    ProductId = ci.ProductId,
                    Quantity = ci.Quantity,
                    Price = ci.Product.Price // Capture price at time of purchase
                }).ToList();

                // Add order to database
                _context.Orders.Add(order);

                // Clear the shopping cart
                _context.CartItems.RemoveRange(cart.CartItems);

                // Update product stock quantities
                foreach (var cartItem in cart.CartItems)
                {
                    var product = cartItem.Product;
                    product.StockQuantity -= cartItem.Quantity;
                    
                    if (product.StockQuantity < 0)
                    {
                        _logger.LogWarning($"Product {product.Id} stock went negative: {product.StockQuantity}");
                        product.StockQuantity = 0;
                    }
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Order {order.Id} created successfully for user {userId}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating order for user {userId}: {ex.Message}");
                throw; // Re-throw to return 500 status, causing Stripe to retry
            }
        }

        /// <summary>
        /// Handle failed payments
        /// </summary>
        private async Task HandlePaymentFailed(Event stripeEvent)
        {
            var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
            _logger.LogWarning($"Payment failed for PaymentIntent: {paymentIntent?.Id}");
            
            // Could implement logic to notify user or clean up pending orders
            await Task.CompletedTask;
        }

        /// <summary>
        /// Format address from Stripe address object
        /// </summary>
        private string? FormatAddress(Address? address)
        {
            if (address == null) return null;

            return $"{address.Line1}, " +
                   $"{(!string.IsNullOrEmpty(address.Line2) ? address.Line2 + ", " : "")}" +
                   $"{address.City}, {address.State} {address.PostalCode}, {address.Country}";
        }

    /// <summary>
    /// Test endpoint for webhook functionality (bypasses signature verification)
    /// POST: api/webhook/test
    /// </summary>    [HttpPost("test")]
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
                _context.ShoppingCarts.Remove(cart);

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