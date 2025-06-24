using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using EcommerceAPI.DTOs;

namespace EcommerceAPI.Pages
{
    public class RegisterModel : PageModel
    {
        private readonly UserManager<Models.ApplicationUser> _userManager;
        private readonly SignInManager<Models.ApplicationUser> _signInManager;

        public RegisterModel(UserManager<Models.ApplicationUser> userManager, SignInManager<Models.ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [BindProperty]
        [Required, StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [BindProperty]
        [Required, StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        [BindProperty]
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [BindProperty]
        [Required, StringLength(100, MinimumLength = 6)]
        public string Password { get; set; } = string.Empty;

        [BindProperty]
        [Required, Compare("Password")]
        public string ConfirmPassword { get; set; } = string.Empty;

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var user = new Models.ApplicationUser
            {
                FirstName = FirstName,
                LastName = LastName,
                Email = Email,
                UserName = Email
            };
            var result = await _userManager.CreateAsync(user, Password);
            if (result.Succeeded)
            {
                // Automatically sign in the user
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToPage("/Products");
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return Page();
            }
        }
    }
}
