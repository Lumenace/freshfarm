using freshfarm.Models;  // For AppUser
using freshfarm.Services;  // For EncryptionService
using freshfarm.Data;  // For AuthDbContext
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using AspNetCore.ReCaptcha;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// ✅ Configure Database and Identity
builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("AuthConnectionString")));

// ✅ Configure Identity with Strong Security Policies
builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 12;  // Ensure strong passwords
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.SignIn.RequireConfirmedAccount = true;  // Ensure email confirmation before login

    // ✅ Implement Account Lockout After Multiple Failed Attempts
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 3;
    options.Lockout.AllowedForNewUsers = true;
})
.AddEntityFrameworkStores<AuthDbContext>()
.AddDefaultTokenProviders();

// ✅ Configure Cookie Authentication and Session Management
builder.Services.ConfigureApplicationCookie(options =>
{
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);  // Auto logout after 30 mins
    options.SlidingExpiration = true;
    options.LoginPath = "/Account/Login";  // Redirect to login
    options.LogoutPath = "/Account/Logout";  // Redirect after logout
    options.AccessDeniedPath = "/Account/AccessDenied";  // Handle unauthorized access
});

// ✅ Implement Session Management for Multi-Login Prevention
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);  // Auto logout after inactivity
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ✅ Register Services
builder.Services.AddReCaptcha(builder.Configuration.GetSection("GoogleReCaptcha"));
builder.Services.AddScoped<EncryptionService>();
builder.Services.AddScoped<SessionService>();  
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<AuditLogService>();

// ✅ Add Razor Pages (UI framework for MVC)
builder.Services.AddRazorPages();

var app = builder.Build();

// ✅ Middleware configuration
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();  // Show detailed errors in development
}
else
{
    app.UseExceptionHandler("/Error");  // Handle exceptions gracefully in production
    app.UseHsts();  // Enforce secure connection
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();  // ✅ Enable session management
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.UseStatusCodePagesWithRedirects("/errors/{0}");  // Handle errors like 404

app.Run();
