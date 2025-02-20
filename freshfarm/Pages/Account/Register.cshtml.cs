using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using freshfarm.Models;
using freshfarm.Services;
using freshfarm.ViewModels;

namespace freshfarm.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly EncryptionService _encryptionService;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public string ReCaptchaSiteKey { get; private set; }

        public RegisterModel(UserManager<AppUser> userManager,
                             SignInManager<AppUser> signInManager,
                             EncryptionService encryptionService,
                             IConfiguration configuration,
                             IWebHostEnvironment webHostEnvironment)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _encryptionService = encryptionService;
            _configuration = configuration;
            _webHostEnvironment = webHostEnvironment;
            ReCaptchaSiteKey = _configuration["GoogleReCaptcha:SiteKey"];
        }

        [BindProperty]
        public RegisterViewModel RModel { get; set; }

        [BindProperty]
        public string RecaptchaResponse { get; set; }

        public void OnGet()
        {
            Console.WriteLine("🔹 Register Page Loaded.");
        }

        private async Task<bool> ValidateReCaptchaAsync(string token)
        {
            Console.WriteLine("🔹 Validating reCAPTCHA...");

            using var httpClient = new HttpClient();
            var secretKey = _configuration["GoogleReCaptcha:SecretKey"];

            if (string.IsNullOrEmpty(secretKey))
            {
                Console.WriteLine("❌ reCAPTCHA Secret Key is missing!");
                return false;
            }

            var values = new Dictionary<string, string>
            {
                { "secret", secretKey },
                { "response", token }
            };

            var content = new FormUrlEncodedContent(values);
            var response = await httpClient.PostAsync("https://www.google.com/recaptcha/api/siteverify", content);
            var jsonResponse = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ReCaptchaResponse>(jsonResponse);

            Console.WriteLine($"🔹 reCAPTCHA Response: Success={result.success}, Score={result.score}, Action={result.action}");

            if (!result.success)
            {
                Console.WriteLine("❌ reCAPTCHA validation failed!");
                return false;
            }

            if (result.score < 0.5)
            {
                Console.WriteLine("⚠️ Low reCAPTCHA score, possible bot attempt.");
                return false;
            }

            return true;
        }

        public class ReCaptchaResponse
        {
            public bool success { get; set; }
            public double score { get; set; }
            public string action { get; set; }
            public string challenge_ts { get; set; }
            public string hostname { get; set; }
        }

        private string SanitizeInput(string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;
            return Regex.Replace(HtmlEncoder.Default.Encode(input), "[^\\w\\s.@-]", "");
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Console.WriteLine("🔹 Register form submitted...");

            if (!ModelState.IsValid)
            {
                Console.WriteLine("❌ ModelState is invalid");
                return Page();
            }

            Console.WriteLine("🔹 Checking reCAPTCHA response...");
            if (string.IsNullOrEmpty(RecaptchaResponse) || !await ValidateReCaptchaAsync(RecaptchaResponse))
            {
                Console.WriteLine("❌ reCAPTCHA validation failed");
                ModelState.AddModelError("", "Google reCAPTCHA validation failed. Please try again.");
                return Page();
            }

            Console.WriteLine("✅ reCAPTCHA validation passed!");

            RModel.FullName = SanitizeInput(RModel.FullName);
            RModel.MobileNo = SanitizeInput(RModel.MobileNo);
            RModel.DeliveryAddress = SanitizeInput(RModel.DeliveryAddress);
            RModel.AboutMe = SanitizeInput(RModel.AboutMe);

            Console.WriteLine("✅ Inputs sanitized!");

            var existingUser = await _userManager.FindByEmailAsync(RModel.Email);
            if (existingUser != null)
            {
                Console.WriteLine("❌ Email is already registered!");
                ModelState.AddModelError("", "Email is already registered.");
                return Page();
            }

            var encryptedCC = _encryptionService.Encrypt(RModel.CreditCard);
            Console.WriteLine("✅ Credit Card encrypted!");

            string uniqueFileName = null;
            if (RModel.ProfilePhoto != null)
            {
                var fileExtension = Path.GetExtension(RModel.ProfilePhoto.FileName).ToLower();
                Console.WriteLine($"🔹 File Extension: {fileExtension}");

                if (fileExtension != ".jpg" && fileExtension != ".jpeg")
                {
                    Console.WriteLine("❌ Invalid file type");
                    ModelState.AddModelError("", "Invalid file type. Only .jpg images are allowed.");
                    return Page();
                }

                uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(RModel.ProfilePhoto.FileName)}";
                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "upload");

                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                    Console.WriteLine("✅ Created upload directory.");
                }

                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                try
                {
                    using var fileStream = new FileStream(filePath, FileMode.Create);
                    await RModel.ProfilePhoto.CopyToAsync(fileStream);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error uploading the file. Please try again.");
                    Console.WriteLine($"❌ File upload error: {ex.Message}");
                    return Page();
                }

                Console.WriteLine("✅ File uploaded!");
            }

            var user = new AppUser
            {
                FullName = RModel.FullName,
                Gender = RModel.Gender,
                MobileNo = RModel.MobileNo,
                DeliveryAddress = RModel.DeliveryAddress,
                AboutMe = RModel.AboutMe,
                Email = RModel.Email,
                UserName = RModel.Email,
                EncryptedCreditCard = encryptedCC,
                ProfilePhoto = uniqueFileName
            };

            var result = await _userManager.CreateAsync(user, RModel.Password);
            if (result.Succeeded)
            {
                Console.WriteLine("✅ User registration successful!");
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToPage("/Index");
            }

            foreach (var error in result.Errors)
            {
                Console.WriteLine($"❌ Error: {error.Description}");
                ModelState.AddModelError("", error.Description);
            }

            return Page();
        }
    }
}
