using Microsoft.AspNetCore.Identity;
using EcommerceAPI.Models;

namespace EcommerceAPI.Data;

/// <summary>
/// Database initializer class for seeding roles and default admin user
/// </summary>
public static class DbInitializer
{
    /// <summary>
    /// Seeds the database with default roles and admin user
    /// </summary>
    public static async Task SeedAsync(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        // Seed Roles
        await SeedRolesAsync(roleManager);
        
        // Seed Default Admin User
        await SeedDefaultAdminAsync(userManager);
        
        // Seed Default Test User
        await SeedDefaultTestUserAsync(userManager);
    }

    /// <summary>
    /// Creates default roles: Admin and User
    /// </summary>
    private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        // Create Admin role if it doesn't exist
        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            await roleManager.CreateAsync(new IdentityRole("Admin"));
        }

        // Create User role if it doesn't exist
        if (!await roleManager.RoleExistsAsync("User"))
        {
            await roleManager.CreateAsync(new IdentityRole("User"));
        }
    }

    /// <summary>
    /// Creates a default admin user for initial system access
    /// </summary>
    private static async Task SeedDefaultAdminAsync(UserManager<ApplicationUser> userManager)
    {
        const string adminEmail = "admin@ecommerce.com";
        const string adminPassword = "Admin123!";

        // Check if admin user already exists
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            // Create new admin user
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = "System",
                LastName = "Administrator",
                EmailConfirmed = true // Skip email confirmation for seeded admin
            };

            // Create user with password
            var result = await userManager.CreateAsync(adminUser, adminPassword);
            
            if (result.Succeeded)
            {
                // Assign Admin role to the user
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }
        else
        {
            // Ensure existing admin has the Admin role
            if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }
    }

    /// <summary>
    /// Creates a default test user for testing
    /// </summary>
    private static async Task SeedDefaultTestUserAsync(UserManager<ApplicationUser> userManager)
    {
        const string testEmail = "test@example.com";
        const string testPassword = "Test123!";

        var testUser = await userManager.FindByEmailAsync(testEmail);
        if (testUser == null)
        {
            testUser = new ApplicationUser
            {
                UserName = testEmail,
                Email = testEmail,
                FirstName = "Test",
                LastName = "User",
                EmailConfirmed = true
            };
            var result = await userManager.CreateAsync(testUser, testPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(testUser, "User");
            }
        }
        else
        {
            if (!await userManager.IsInRoleAsync(testUser, "User"))
            {
                await userManager.AddToRoleAsync(testUser, "User");
            }
        }
    }
}
