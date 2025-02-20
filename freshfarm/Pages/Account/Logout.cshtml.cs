using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using freshfarm.Models;
using freshfarm.Services;
using System.Threading.Tasks;

namespace freshfarm.Pages.Account
{
    public class LogoutModel : PageModel
    {
        private readonly SignInManager<AppUser> _signInManager;
        private readonly SessionService _sessionService;
        private readonly AuditLogService _auditLogService;
        private readonly ILogger<LogoutModel> _logger;

        public LogoutModel(SignInManager<AppUser> signInManager,
                           SessionService sessionService,
                           AuditLogService auditLogService,
                           ILogger<LogoutModel> logger)
        {
            _signInManager = signInManager;
            _sessionService = sessionService;
            _auditLogService = auditLogService;
            _logger = logger;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _signInManager.UserManager.GetUserAsync(User);
            if (user != null)
            {
                // ✅ Remove session tracking
                _sessionService.RemoveSession(user.Id);

                // ✅ Log user logout event
                await _auditLogService.LogActionAsync(user.Id, "User Logged Out");
            }

            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");
            return RedirectToPage("/Account/Login");
        }
    }
}
