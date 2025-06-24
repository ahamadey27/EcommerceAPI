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
using EcommerceAPI.Data;
using EcommerceAPI.Models;
using EcommerceAPI.DTOs;

namespace EcommerceAPI.Controllers
{
    /// <summary>
    /// Controller for managing user shopping carts
    /// All endpoints require authentication - users can only access their own carts
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // All cart operations require authentication
    public class ShoppingCartController : ControllerBase
    {
        private readonly ILogger<ShoppingCartController> _logger;
        private readonly ApplicationDbContext _context;

        public ShoppingCartController(ILogger<ShoppingCartController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        /// <summary>
        /// Get current user's shopping cart
        /// GET: api/shoppingcart
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<CartResponseDto>> GetCart()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized("User not found in token.");

            var cart = await GetOrCreateUserCartAsync(userId);
            
            var cartDto = await MapCartToResponseDto(cart);
            return Ok(cartDto);
        }

        /// <summary>
        /// Add item to shopping cart
        /// POST: api/shoppingcart/items
        /// </summary>
        [HttpPost("items")]
        public async Task<ActionResult<CartResponseDto>> AddToCart([FromBody] AddToCartDto addToCartDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized("User not found in token.");

            // Verify product exists and has sufficient stock
            var product = await _context.Products.FindAsync(addToCartDto.ProductId);
            if (product == null)
                return NotFound($"Product with ID {addToCartDto.ProductId} not found.");

            if (product.StockQuantity < addToCartDto.Quantity)
                return BadRequest($"Insufficient stock. Only {product.StockQuantity} items available.");

            // Get or create user's cart
            var cart = await GetOrCreateUserCartAsync(userId);

            // Check if item already exists in cart
            var existingCartItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == addToCartDto.ProductId);
            
            if (existingCartItem != null)
            {
                // Update existing item quantity
                var newQuantity = existingCartItem.Quantity + addToCartDto.Quantity;
                if (newQuantity > product.StockQuantity)
                    return BadRequest($"Cannot add {addToCartDto.Quantity} more items. Total would exceed available stock of {product.StockQuantity}.");
                
                existingCartItem.Quantity = newQuantity;
                existingCartItem.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                // Add new item to cart
                var cartItem = new CartItem
                {
                    CartId = cart.Id,
                    ProductId = addToCartDto.ProductId,
                    Quantity = addToCartDto.Quantity,
                    CreatedAt = DateTime.UtcNow
                };
                
                cart.CartItems.Add(cartItem);
            }

            cart.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var cartDto = await MapCartToResponseDto(cart);
            return Ok(cartDto);
        }

        /// <summary>
        /// Update quantity of specific item in cart
        /// PUT: api/shoppingcart/items/{productId}
        /// </summary>
        [HttpPut("items/{productId}")]
        public async Task<ActionResult<CartResponseDto>> UpdateCartItem(int productId, [FromBody] UpdateCartItemDto updateDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized("User not found in token.");

            var cart = await GetUserCartAsync(userId);
            if (cart == null)
                return NotFound("Shopping cart not found.");

            var cartItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == productId);
            if (cartItem == null)
                return NotFound($"Product with ID {productId} not found in cart.");

            // Verify stock availability
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
                return NotFound("Product no longer exists.");

            if (product.StockQuantity < updateDto.Quantity)
                return BadRequest($"Insufficient stock. Only {product.StockQuantity} items available.");

            cartItem.Quantity = updateDto.Quantity;
            cartItem.UpdatedAt = DateTime.UtcNow;
            cart.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var cartDto = await MapCartToResponseDto(cart);
            return Ok(cartDto);
        }

        /// <summary>
        /// Remove specific item from cart
        /// DELETE: api/shoppingcart/items/{productId}
        /// </summary>
        [HttpDelete("items/{productId}")]
        public async Task<ActionResult<CartResponseDto>> RemoveFromCart(int productId)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized("User not found in token.");

            var cart = await GetUserCartAsync(userId);
            if (cart == null)
                return NotFound("Shopping cart not found.");

            var cartItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == productId);
            if (cartItem == null)
                return NotFound($"Product with ID {productId} not found in cart.");

            cart.CartItems.Remove(cartItem);
            cart.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var cartDto = await MapCartToResponseDto(cart);
            return Ok(cartDto);
        }

        /// <summary>
        /// Clear all items from cart
        /// DELETE: api/shoppingcart
        /// </summary>
        [HttpDelete]
        public async Task<IActionResult> ClearCart()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized("User not found in token.");

            var cart = await GetUserCartAsync(userId);
            if (cart == null)
                return NotFound("Shopping cart not found.");

            cart.CartItems.Clear();
            cart.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Helper methods
        private string? GetCurrentUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        private async Task<ShoppingCart> GetOrCreateUserCartAsync(string userId)
        {
            var cart = await GetUserCartAsync(userId);
            
            if (cart == null)
            {
                cart = new ShoppingCart
                {
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                
                _context.ShoppingCarts.Add(cart);
                await _context.SaveChangesAsync();
            }
            
            return cart;
        }

        private async Task<ShoppingCart?> GetUserCartAsync(string userId)
        {
            return await _context.ShoppingCarts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);
        }

        private async Task<CartResponseDto> MapCartToResponseDto(ShoppingCart cart)
        {
            // Ensure cart items are loaded with products
            if (!cart.CartItems.Any() || cart.CartItems.First().Product == null)
            {
                cart = await _context.ShoppingCarts
                    .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                    .FirstAsync(c => c.Id == cart.Id);
            }

            var cartItems = cart.CartItems.Select(ci => new CartItemResponseDto
            {
                Id = ci.Id,
                ProductId = ci.ProductId,
                ProductName = ci.Product.Name,
                ProductPrice = ci.Product.Price,
                Quantity = ci.Quantity,
                TotalPrice = ci.Product.Price * ci.Quantity,
                ProductImageUrl = ci.Product.ImageUrl,
                ProductCategory = ci.Product.Category
            }).ToList();

            return new CartResponseDto
            {
                Id = cart.Id,
                UserId = cart.UserId,
                Items = cartItems,
                TotalAmount = cartItems.Sum(ci => ci.TotalPrice),
                TotalItems = cartItems.Sum(ci => ci.Quantity),
                CreatedAt = cart.CreatedAt,
                UpdatedAt = cart.UpdatedAt
            };
        }
    }
}