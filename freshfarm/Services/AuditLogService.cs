using freshfarm.Data;
using freshfarm.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace freshfarm.Services
{
    public class AuditLogService
    {
        private readonly AuthDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditLogService(AuthDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task LogActionAsync(string userId, string action)
        {
            var ipAddress = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            var logEntry = new AuditLog
            {
                UserId = userId,
                Action = action,
                IPAddress = ipAddress
            };

            _context.AuditLogs.Add(logEntry);
            await _context.SaveChangesAsync();
        }
    }
}
