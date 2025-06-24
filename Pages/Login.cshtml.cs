using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace EcommerceAPI.Pages
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<Models.ApplicationUser> _signInManager;
        public LoginModel(SignInManager<Models.ApplicationUser> signInManager)
        {
            _signInManager = signInManager;
        }

        [BindProperty]
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [BindProperty]
        [Required]
        public string Password { get; set; } = string.Empty;

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();
            var result = await _signInManager.PasswordSignInAsync(Email, Password, false, false);
            if (result.Succeeded)
            {
                return RedirectToPage("/Products");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return Page();
            }
        }
    }
}
