using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using freshfarm.Models;

namespace freshfarm.Pages.Account
{
    public class TwoFactorAuthenticationModel : PageModel
    {
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;
        private readonly ILogger<TwoFactorAuthenticationModel> _logger;

        public TwoFactorAuthenticationModel(SignInManager<AppUser> signInManager,
                                            UserManager<AppUser> userManager,
                                            ILogger<TwoFactorAuthenticationModel> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
        }

        [BindProperty]
        public string Code { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                ModelState.AddModelError("", "Invalid authentication request.");
                return Page();
            }

            var result = await _signInManager.TwoFactorSignInAsync("Email", Code, isPersistent: false, rememberClient: false);
            if (result.Succeeded)
            {
                _logger.LogInformation($"User {user.Email} authenticated with 2FA.");
                return RedirectToPage("/Index");
            }

            ModelState.AddModelError("", "Invalid authentication code.");
            return Page();
        }
    }
}
