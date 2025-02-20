using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using freshfarm.Models;
using freshfarm.Services;
using freshfarm.ViewModels;

namespace freshfarm.Pages.Account
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly EmailService _emailService;
        private readonly ILogger<ForgotPasswordModel> _logger;

        public ForgotPasswordModel(UserManager<AppUser> userManager,
                                   EmailService emailService,
                                   ILogger<ForgotPasswordModel> logger)
        {
            _userManager = userManager;
            _emailService = emailService;
            _logger = logger;
        }

        [BindProperty]
        public ForgotPasswordViewModel FPModel { get; set; }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var user = await _userManager.FindByEmailAsync(FPModel.Email);
            if (user == null)
            {
                ModelState.AddModelError("", "If an account exists with this email, you will receive a password reset link.");
                return Page();
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var encryptedToken = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(token));
            var resetLink = Url.Page("/Account/ResetPassword", null, new { email = user.Email, token = encryptedToken }, Request.Scheme);

            await _emailService.SendEmailAsync(user.Email, "Reset Your Password",
                $"Click <a href='{resetLink}'>here</a> to reset your password.");

            _logger.LogInformation($"Password reset email sent to {user.Email}.");
            return RedirectToPage("/Account/Login");
        }
    }
}
