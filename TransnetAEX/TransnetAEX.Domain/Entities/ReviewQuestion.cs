
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TransnetAEX.Domain.Entities
{
    public class ReviewQuestion
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(30)]
        public string SectionType { get; set; } = "Employee"; // Employee / LineManager

        [Required]
        [MaxLength(300)]
        public string QuestionText { get; set; } = string.Empty;

        public string? Response { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedDate { get; set; }

        [ForeignKey(nameof(Review))]
        public int ReviewId { get; set; }
        public HRInitiatedReview Review { get; set; } = default!;
    }
}
