using TransnetAEX.Domain.Entities;

namespace TransnetAEX.Api.DTOs.Review
{
    public class ReviewResponseDto
    {
        public int Id { get; set; }
        public string HrEmail { get; set; } = string.Empty;
        public string EmployeeEmail { get; set; } = string.Empty;
        public string LineManagerEmail { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }

        public List<ReviewSection>? ReviewSections { get; set; }
        public List<NotificationLog>? Notifications { get; set; }
    }
}
