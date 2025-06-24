using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;

namespace EcommerceAPI.Pages
{
    public class LogoutModel : PageModel
    {
        private readonly SignInManager<Models.ApplicationUser> _signInManager;
        public LogoutModel(SignInManager<Models.ApplicationUser> signInManager)
        {
            _signInManager = signInManager;
        }
        public async Task<IActionResult> OnPost()
        {
            await _signInManager.SignOutAsync();
            return RedirectToPage("/Index");
        }
    }
}
