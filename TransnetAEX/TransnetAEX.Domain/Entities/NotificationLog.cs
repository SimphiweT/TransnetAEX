
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TransnetAEX.Domain.Entities
{
    public class NotificationLog
    {
        [Key]
        public int NotificationId { get; set; }

        [Required, EmailAddress]
        public string RecipientEmail { get; set; } = string.Empty;

        [MaxLength(50)]
        public string NotificationType { get; set; } = string.Empty;
        // EmployeeLink, ManagerLink, Reminder, Closure

        public DateTime SentDate { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = "Sent";

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedDate { get; set; }

        // Navigation
        [ForeignKey(nameof(Review))]
        public int ReviewId { get; set; }
        public HRInitiatedReview Review { get; set; } = default!;
    }
}
