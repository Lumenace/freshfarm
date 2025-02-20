using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using freshfarm.Models;
using freshfarm.ViewModels;

namespace freshfarm.Pages.Account
{
    public class ResetPasswordModel : PageModel
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ILogger<ResetPasswordModel> _logger;

        public ResetPasswordModel(UserManager<AppUser> userManager, 
                                  ILogger<ResetPasswordModel> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        [BindProperty]
        public ResetPasswordViewModel RPModel { get; set; }

        public void OnGet(string email, string token)
        {
            RPModel = new ResetPasswordViewModel { Email = email, Token = token };
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var user = await _userManager.FindByEmailAsync(RPModel.Email);
            if (user == null)
            {
                ModelState.AddModelError("", "Invalid request.");
                return Page();
            }

            var result = await _userManager.ResetPasswordAsync(user, RPModel.Token, RPModel.NewPassword);
            if (result.Succeeded)
            {
                _logger.LogInformation($"Password reset successfully for {user.Email}.");
                return RedirectToPage("/Account/Login");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return Page();
        }
    }
}
