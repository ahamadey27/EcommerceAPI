 using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EcommerceAPI.Data;
using EcommerceAPI.Models;
using EcommerceAPI.DTOs;

namespace EcommerceAPI.Controllers;

/// <summary>
/// Controller for managing products with role-based security
/// Demonstrates three levels of authorization:
/// 1. Public endpoints (anonymous access for browsing)
/// 2. Admin-only endpoints (create, update, delete)
/// 3. Authorized endpoints (future user-specific features)
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ProductsController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get all products - PUBLIC ACCESS
    /// Anyone can browse products without authentication
    /// GET: api/products
    /// </summary>
    [HttpGet]
    [AllowAnonymous] // Public endpoint - no authentication required
    public async Task<ActionResult<IEnumerable<ProductResponseDto>>> GetProducts()
    {
        var products = await _context.Products
            .Select(p => new ProductResponseDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                StockQuantity = p.StockQuantity,
                Category = p.Category,
                ImageUrl = p.ImageUrl,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            })
            .ToListAsync();

        return Ok(products);
    }

    /// <summary>
    /// Get a specific product by ID - PUBLIC ACCESS
    /// Anyone can view individual product details
    /// GET: api/products/{id}
    /// </summary>
    [HttpGet("{id}")]
    [AllowAnonymous] // Public endpoint - no authentication required
    public async Task<ActionResult<ProductResponseDto>> GetProduct(int id)
    {
        var product = await _context.Products.FindAsync(id);

        if (product == null)
        {
            return NotFound($"Product with ID {id} not found.");
        }

        var productDto = new ProductResponseDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            StockQuantity = product.StockQuantity,
            Category = product.Category,
            ImageUrl = product.ImageUrl,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt
        };

        return Ok(productDto);
    }

    /// <summary>
    /// Create a new product - ADMIN ONLY
    /// Only administrators can add new products to the catalog
    /// POST: api/products
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")] // Admin-only endpoint
    public async Task<ActionResult<ProductResponseDto>> CreateProduct([FromBody] CreateProductDto productDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Create new product entity from DTO
        var product = new Product
        {
            Name = productDto.Name,
            Description = productDto.Description,
            Price = productDto.Price,
            StockQuantity = productDto.StockQuantity,
            Category = productDto.Category,
            ImageUrl = productDto.ImageUrl,
            CreatedAt = DateTime.UtcNow
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        // Return the created product as DTO
        var responseDto = new ProductResponseDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            StockQuantity = product.StockQuantity,
            Category = product.Category,
            ImageUrl = product.ImageUrl,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt
        };

        return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, responseDto);
    }

    /// <summary>
    /// Update an existing product - ADMIN ONLY
    /// Only administrators can modify product information
    /// PUT: api/products/{id}
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")] // Admin-only endpoint
    public async Task<IActionResult> UpdateProduct(int id, [FromBody] UpdateProductDto productDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            return NotFound($"Product with ID {id} not found.");
        }

        // Update product properties
        product.Name = productDto.Name;
        product.Description = productDto.Description;
        product.Price = productDto.Price;
        product.StockQuantity = productDto.StockQuantity;
        product.Category = productDto.Category;
        product.ImageUrl = productDto.ImageUrl;
        product.UpdatedAt = DateTime.UtcNow;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            // Handle concurrency conflicts if needed
            if (!ProductExists(id))
            {
                return NotFound();
            }
            throw;
        }

        return NoContent(); // 204 No Content - successful update
    }

    /// <summary>
    /// Delete a product - ADMIN ONLY
    /// Only administrators can remove products from the catalog
    /// DELETE: api/products/{id}
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")] // Admin-only endpoint
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            return NotFound($"Product with ID {id} not found.");
        }

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();

        return NoContent(); // 204 No Content - successful deletion
    }

    /// <summary>
    /// Helper method to check if a product exists
    /// </summary>
    private bool ProductExists(int id)
    {
        return _context.Products.Any(e => e.Id == id);
    }
}
