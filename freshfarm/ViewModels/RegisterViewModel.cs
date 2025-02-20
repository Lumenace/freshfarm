using System.ComponentModel.DataAnnotations;

namespace freshfarm.ViewModels
{
    public class RegisterViewModel
    {
        [Required]
        public string FullName { get; set; }

        [Required]
        public string Gender { get; set; }

        [Required]
        public string MobileNo { get; set; }

        [Required]
        public string DeliveryAddress { get; set; }

        [Required]
        public string AboutMe { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [MinLength(12, ErrorMessage = "Password must be at least 12 characters.")]
        public string Password { get; set; }

        [Required]
        [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; }

        [Required]
        public string CreditCard { get; set; }

        public IFormFile ProfilePhoto { get; set; }
    }
}
