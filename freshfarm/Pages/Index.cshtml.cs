using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using freshfarm.Models;
using freshfarm.Services;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace freshfarm.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly UserManager<AppUser> _userManager;
        private readonly EncryptionService _encryptionService;
        private readonly SignInManager<AppUser> _signInManager;

        public IndexModel(ILogger<IndexModel> logger,
                          UserManager<AppUser> userManager,
                          EncryptionService encryptionService,
                          SignInManager<AppUser> signInManager)
        {
            _logger = logger;
            _userManager = userManager;
            _encryptionService = encryptionService;
            _signInManager = signInManager;
        }

        public AppUser? CurrentUser { get; set; }
        public string? DecryptedCreditCard { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            // Get current logged-in user
            CurrentUser = await _userManager.GetUserAsync(User);

            if (CurrentUser == null)
            {
                return RedirectToPage("/Account/Login");  // Redirect unauthenticated users
            }

            // Decrypt Credit Card Number (if exists)
            if (CurrentUser.EncryptedCreditCard != null)
            {
                DecryptedCreditCard = _encryptionService.Decrypt(CurrentUser.EncryptedCreditCard);
            }

            return Page();
        }

        public async Task<IActionResult> OnPostLogoutAsync()
        {
            await _signInManager.SignOutAsync();
            return RedirectToPage("/Account/Login");
        }
    }
}
