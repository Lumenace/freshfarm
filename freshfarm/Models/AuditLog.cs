using System;
using System.ComponentModel.DataAnnotations;

namespace freshfarm.Models
{
    public class AuditLog
    {
        [Key]
        public int Id { get; set; }
        public string UserId { get; set; } // Store the user's ID
        public string Action { get; set; } // Login, Logout, Failed Login, etc.
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string IPAddress { get; set; } // Store user's IP Address
    }
}
