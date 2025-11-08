using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TransnetAEX.Domain.Entities
{
    /// <summary>
    /// Records user session events for audit and traceability.
    /// </summary>
    public class SessionAuditLog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [MaxLength(120)]
        public string DisplayName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? UserRole { get; set; }

        [MaxLength(50)]
        public string ActionType { get; set; } = string.Empty; // "Login", "Refresh", "Logout"

        [MaxLength(50)]
        public string? IpAddress { get; set; }

        [MaxLength(300)]
        public string? DeviceInfo { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public string? Notes { get; set; }
        public bool Success { get; set; } = true;

        [MaxLength(300)]
        public string? FailureReason { get; set; }

    }
}
