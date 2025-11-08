using System.ComponentModel.DataAnnotations;

namespace TransnetAEX.Domain.Entities
{
    public class HRInitiatedReview   // Changed to public
    {
        [Key]
        public int Id { get; set; }

        [Required, EmailAddress]
        public string HrEmail { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string EmployeeEmail { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string LineManagerEmail { get; set; } = string.Empty;

        [MaxLength(30)]
        public string Status { get; set; } = "Pending"; // Pending, InProgress, Completed, Closed

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedDate { get; set; }

        // Navigation
        public ICollection<ReviewSection> ReviewSections { get; set; } = new List<ReviewSection>();
        public ICollection<QuestionBank> Questions { get; set; } = new List<QuestionBank>();
        public ICollection<NotificationLog> Notifications { get; set; } = new List<NotificationLog>();
    }
}
