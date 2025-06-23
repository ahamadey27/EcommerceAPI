using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EcommerceAPI.Models;
using EcommerceAPI.DTOs;

namespace EcommerceAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    // Dependency injection for Identity services and configuration
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IConfiguration _configuration;

    // Constructor - ASP.NET Core automatically injects these services
    public AuthController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
    }    /// <summary>
    /// Registers a new user and returns a JWT token
    /// POST: api/auth/register
    /// </summary>
    [HttpPost("register")]
    [AllowAnonymous] // Allow anonymous access - users need this to create accounts
    public async Task<IActionResult> Register([FromBody] RegisterDto model)
    {
        // Validate incoming data using model validation attributes
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // Create new ApplicationUser object from registration data
        var user = new ApplicationUser
        {
            UserName = model.Email,    // Use email as username
            Email = model.Email,
            FirstName = model.FirstName,
            LastName = model.LastName
        };

        // Use Identity's UserManager to create user with hashed password
        var result = await _userManager.CreateAsync(user, model.Password);

        // Check if user creation failed (duplicate email, weak password, etc.)
        if (!result.Succeeded)
        {
            // Add Identity errors to ModelState for client feedback
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
            return BadRequest(ModelState);
        }

        // User created successfully - generate JWT token for immediate login
        var token = await GenerateJwtToken(user);

        // Return success response with token and user info
        return Ok(new AuthResponseDto
        {
            Token = token,
            Email = user.Email!,
            FirstName = user.FirstName,
            LastName = user.LastName,
            ExpiresAt = DateTime.UtcNow.AddMinutes(int.Parse(_configuration["JWT:ExpiryInMinutes"]!))
        });
    }    /// <summary>
    /// Authenticates user and returns JWT token
    /// POST: api/auth/login
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous] // Allow anonymous access - users need this to get JWT tokens
    public async Task<IActionResult> Login([FromBody] LoginDto model)
    {
        // Validate incoming login data
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // Look up user by email address
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
            return Unauthorized("Invalid email or password.");

        // Verify password using Identity's secure password checking
        // Third parameter 'false' means don't lockout on failure for this check
        var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
        if (!result.Succeeded)
            return Unauthorized("Invalid email or password.");

        // Authentication successful - generate JWT token
        var token = await GenerateJwtToken(user);

        // Return success response with token and user info
        return Ok(new AuthResponseDto
        {
            Token = token,
            Email = user.Email!,
            FirstName = user.FirstName,
            LastName = user.LastName,
            ExpiresAt = DateTime.UtcNow.AddMinutes(int.Parse(_configuration["JWT:ExpiryInMinutes"]!))
        });
    }

    /// <summary>
    /// Private helper method to generate JWT tokens with user claims
    /// </summary>
    private async Task<string> GenerateJwtToken(ApplicationUser user)
    {
        // Get JWT settings from appsettings.json
        var jwtSecret = _configuration["JWT:Secret"]!;
        var jwtIssuer = _configuration["JWT:Issuer"]!;
        var jwtAudience = _configuration["JWT:Audience"]!;
        var jwtExpiryInMinutes = int.Parse(_configuration["JWT:ExpiryInMinutes"]!);

        // Build claims - these are pieces of information stored inside the JWT
        // Claims allow the API to know who the user is without database lookups
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),        // User's unique ID
            new Claim(ClaimTypes.Email, user.Email!),             // User's email
            new Claim(ClaimTypes.GivenName, user.FirstName),      // First name
            new Claim(ClaimTypes.Surname, user.LastName),         // Last name
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) // Unique token ID
        };

        // Add user's roles to claims (for authorization)
        var roles = await _userManager.GetRolesAsync(user);
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        // Create cryptographic key from secret for signing the token
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // Build the actual JWT token with all components
        var token = new JwtSecurityToken(
            issuer: jwtIssuer,                                    // Who issued this token
            audience: jwtAudience,                                // Who this token is for
            claims: claims,                                       // User information
            expires: DateTime.UtcNow.AddMinutes(jwtExpiryInMinutes), // When token expires
            signingCredentials: credentials                       // Cryptographic signature
        );

        // Convert token object to string format for transmission
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}