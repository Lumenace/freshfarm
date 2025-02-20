using System.ComponentModel.DataAnnotations;

namespace freshfarm.ViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
