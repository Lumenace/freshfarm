using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace freshfarm.Services
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        private string MaskEmail(string email)
        {
            var atIndex = email.IndexOf('@');
            if (atIndex <= 1) return email; // Not enough characters to mask
            return email.Substring(0, 1) + new string('*', atIndex - 1) + email.Substring(atIndex);
        }

        public async Task SendEmailAsync(string email, string subject, string message, bool isHtml = true)
        {
            try
            {
                // Load SMTP configuration from appsettings.json
                var smtpServer = _configuration["EmailSettings:SmtpServer"];
                var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"]);
                var smtpUser = _configuration["EmailSettings:SmtpUser"];
                var smtpPass = _configuration["EmailSettings:SmtpPass"];
                var senderEmail = _configuration["EmailSettings:SenderEmail"];
                var enableSsl = bool.Parse(_configuration["EmailSettings:EnableSsl"] ?? "true");

                using var smtpClient = new SmtpClient(smtpServer)
                {
                    Port = smtpPort,
                    Credentials = new NetworkCredential(smtpUser, smtpPass),
                    EnableSsl = enableSsl,
                };

                using var mailMessage = new MailMessage
                {
                    From = new MailAddress(senderEmail),
                    Subject = subject,
                    Body = message,
                    IsBodyHtml = isHtml, // Can send plain text or HTML emails
                    Priority = MailPriority.High
                };

                mailMessage.To.Add(email);

                await smtpClient.SendMailAsync(mailMessage);
                _logger.LogInformation($"✅ Email successfully sent to {MaskEmail(email)} with subject: {subject}");
            }
            catch (SmtpException smtpEx)
            {
                _logger.LogError($"❌ SMTP error sending email to {email}: {smtpEx.StatusCode} - {smtpEx.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Unexpected error sending email to {MaskEmail(email)}: {ex.Message}");
            }
        }
    }
}
