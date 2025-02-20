using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace freshfarm.Pages
{
    public class ErrorModel : PageModel
    {
        private readonly ILogger<ErrorModel> _logger;

        public ErrorModel(ILogger<ErrorModel> logger)
        {
            _logger = logger;
        }

        public int StatusCode { get; set; }

        public void OnGet(int code)
        {
            StatusCode = code;
            _logger.LogError($"Error encountered: {code}");
        }
    }
}
