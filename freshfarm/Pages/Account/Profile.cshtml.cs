using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using freshfarm.Models;
using System.Threading.Tasks;

namespace freshfarm.Pages.Account
{
    public class ProfileModel : PageModel
    {
        private readonly UserManager<AppUser> _userManager;

        public ProfileModel(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        public AppUser CurrentUser { get; set; }

        public async Task OnGetAsync()
        {
            CurrentUser = await _userManager.GetUserAsync(User);
        }
    }
}
