using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TransnetAEX.Domain.Entities
{
    public class ReviewSection
    {
        [Key]
        public int SectionId { get; set; }

        [Required]
        [MaxLength(30)]
        public string SectionType { get; set; } = string.Empty; // Employee, LineManager, HR

        public string? ResponsesJson { get; set; } // Stores questionId-answer pairs
        public bool IsCompleted { get; set; } = false;
        public DateTime? CompletedDate { get; set; }

        [EmailAddress]
        public string ReviewerEmail { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedDate { get; set; }

        // Navigation
        [ForeignKey(nameof(Review))]
        public int ReviewId { get; set; }
        public HRInitiatedReview Review { get; set; } = default!;

        public ICollection<QuestionBank> Questions { get; set; } = new List<QuestionBank>();
    }
}
