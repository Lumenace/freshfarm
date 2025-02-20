using Microsoft.AspNetCore.Identity;

namespace freshfarm.Models
{
    public class AppUser : IdentityUser
    {
        public string FullName { get; set; }
        public string Gender { get; set; }
        public string MobileNo { get; set; }
        public string DeliveryAddress { get; set; }
        public string AboutMe { get; set; }
        public string ProfilePhoto { get; set; }  // Optional: Store file path
        public byte[] EncryptedCreditCard { get; set; }  // Encrypted credit card data
    }
}
