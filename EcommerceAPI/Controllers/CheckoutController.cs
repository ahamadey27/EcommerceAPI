using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using Stripe.Checkout;
using EcommerceAPI.Data;
using EcommerceAPI.DTOs;

namespace EcommerceAPI.Controllers
{
    /// <summary>
    /// Controller for handling Stripe checkout process
    /// Converts shopping cart to Stripe payment session
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Only authenticated users can checkout
    public class CheckoutController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<CheckoutController> _logger;

        public CheckoutController(ApplicationDbContext context, IConfiguration configuration, ILogger<CheckoutController> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Create Stripe checkout session from user's cart
        /// POST: api/checkout/create-session
        /// </summary>
        [HttpPost("create-session")]
        public async Task<ActionResult<CheckoutSessionResponseDto>> CreateCheckoutSession()
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

            // Create Stripe line items from cart
            var lineItems = cart.CartItems.Select(ci => new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    UnitAmount = (long)(ci.Product.Price * 100), // Stripe uses cents
                    Currency = "usd",
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = ci.Product.Name,
                        Description = ci.Product.Description,
                        Images = ci.Product.ImageUrl != null ? new List<string> { ci.Product.ImageUrl } : null,
                    },
                },
                Quantity = ci.Quantity,
            }).ToList();

            // Configure checkout session options
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = lineItems,
                Mode = "payment",
                ClientReferenceId = userId, // Link payment to user
                SuccessUrl = _configuration["Stripe:SuccessUrl"] ?? "http://localhost:3000/success?session_id={CHECKOUT_SESSION_ID}",
                CancelUrl = _configuration["Stripe:CancelUrl"] ?? "http://localhost:3000/cancel",
                CustomerEmail = User.FindFirst(ClaimTypes.Email)?.Value,
                BillingAddressCollection = "required",
                ShippingAddressCollection = new SessionShippingAddressCollectionOptions
                {
                    AllowedCountries = new List<string> { "US", "CA" },
                },
            };

            // Create the session
            var service = new SessionService();
            var session = await service.CreateAsync(options);

            return Ok(new CheckoutSessionResponseDto
            {
                SessionId = session.Id,
                CheckoutUrl = session.Url,
                PublishableKey = _configuration["Stripe:PublishableKey"]!
            });
        }

        /// <summary>
        /// Get checkout session details
        /// GET: api/checkout/session/{sessionId}
        /// </summary>
        [HttpGet("session/{sessionId}")]
        public async Task<ActionResult<CheckoutSessionDetailsDto>> GetCheckoutSession(string sessionId)
        {
            try
            {
                var service = new SessionService();
                var session = await service.GetAsync(sessionId);

                return Ok(new CheckoutSessionDetailsDto
                {
                    SessionId = session.Id,
                    PaymentStatus = session.PaymentStatus,
                    PaymentIntentId = session.PaymentIntentId,
                    AmountTotal = session.AmountTotal,
                    Currency = session.Currency,
                    CustomerEmail = session.CustomerEmail
                });
            }
            catch (Exception ex)
            {
                return BadRequest($"Invalid session ID: {ex.Message}");
            }
        }

        private string? GetCurrentUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }        /// <summary>
        /// Get checkout information and available options
        /// </summary>
        /// <returns>Checkout information</returns>
        [HttpGet("info")]
        public IActionResult GetCheckoutInfo()
        {
            return Ok(new { 
                message = "Checkout API ready", 
                availableEndpoints = new[] { 
                    "POST /api/checkout/create-session - Create Stripe checkout session",
                    "GET /api/checkout/session/{sessionId} - Get session details" 
                }
            });
        }
    }
}