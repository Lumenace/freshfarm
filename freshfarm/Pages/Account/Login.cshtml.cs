using System.Text.Encodings.Web;
using freshfarm.ViewModels;
using freshfarm.Models;
using freshfarm.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace freshfarm.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;
        private readonly SessionService _sessionService;
        private readonly EmailService _emailService;
        private readonly AuditLogService _auditLogService;
        private readonly ILogger<LoginModel> _logger;

        public LoginModel(SignInManager<AppUser> signInManager,
                          UserManager<AppUser> userManager,
                          SessionService sessionService,
                          EmailService emailService,
                          AuditLogService auditLogService,
                          ILogger<LoginModel> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _sessionService = sessionService;
            _emailService = emailService;
            _auditLogService = auditLogService;
            _logger = logger;
        }

        [BindProperty]
        public LoginViewModel LoginData { get; set; }

        public void OnGet() { }

        // ✅ Secure Input Sanitization Function
        private string SanitizeInput(string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;

            // ✅ Prevent XSS by encoding input
            return HtmlEncoder.Default.Encode(input);
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            // ✅ Apply Input Sanitization
            
            

            var user = await _userManager.FindByEmailAsync(LoginData.Email);
            if (user == null)
            {
                await _auditLogService.LogActionAsync("Unknown", "Failed Login Attempt - Email Not Found");
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return Page();
            }

            // ✅ Handle Locked Accounts - Redirect to Lockout Page
            if (await _userManager.IsLockedOutAsync(user))
            {
                _logger.LogWarning($"User {user.Email} account locked due to multiple failed attempts.");
                await _auditLogService.LogActionAsync(user.Id, "Account Locked");
                return RedirectToPage("/Account/Lockout");
            }

            string sessionId = HttpContext.Session.Id;

            // ✅ Prevent multiple logins from different devices
            if (_sessionService.IsUserLoggedInElsewhere(user.Id, sessionId))
            {
                await _auditLogService.LogActionAsync(user.Id, "Blocked Login - Multiple Sessions Detected");
                ModelState.AddModelError(string.Empty, "You are already logged in from another device.");
                return Page();
            }

            var result = await _signInManager.PasswordSignInAsync(
                LoginData.Email, LoginData.Password, LoginData.RememberMe, lockoutOnFailure: true);

            // ✅ If 2FA is required, send a verification code
            if (result.RequiresTwoFactor)
            {
                var token = await _userManager.GenerateTwoFactorTokenAsync(user, "Email");
                await _emailService.SendEmailAsync(user.Email, "Your 2FA Code", $"Your 2FA code is: {token}");
                await _auditLogService.LogActionAsync(user.Id, "2FA Required");
                return RedirectToPage("/Account/TwoFactorAuthentication");
            }

            // ✅ Successful Login
            if (result.Succeeded)
            {
                _sessionService.SetUserSession(user.Id, sessionId);
                _logger.LogInformation($"User {user.Email} logged in successfully.");
                await _auditLogService.LogActionAsync(user.Id, "Login Successful");
                return RedirectToPage("/Index");
            }

            // ✅ Failed Login Attempt - Track and Lockout if Necessary
            if (result.IsLockedOut)
            {
                _logger.LogWarning($"User {user.Email} account locked due to multiple failed attempts.");
                await _auditLogService.LogActionAsync(user.Id, "Account Locked - Too Many Failed Attempts");
                return RedirectToPage("/Account/Lockout");
            }
            else
            {
                _logger.LogWarning($"Failed login attempt for {user.Email}.");
                await _auditLogService.LogActionAsync(user.Id, "Failed Login Attempt");
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }

            return Page();
        }
    }
}
