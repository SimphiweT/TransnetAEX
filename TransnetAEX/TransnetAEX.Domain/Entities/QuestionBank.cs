
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TransnetAEX.Domain.Entities
{
    public class QuestionBank
    {
        [Key]
        public int QuestionId { get; set; }

        [Required]
        [MaxLength(30)]
        public string SectionType { get; set; } = string.Empty; // Employee / LineManager / HR

        [Required]
        [MaxLength(300)]
        public string QuestionText { get; set; } = string.Empty;

        [MaxLength(20)]
        public string InputType { get; set; } = "Text";
        // "Text", "TextArea", "Rating", "Dropdown", "Checkbox"

        public string? Options { get; set; } // e.g. "Poor,Fair,Good,Excellent"

        public bool IsRequired { get; set; } = false;
        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedDate { get; set; }

        // Navigation (optional)
        public int? SectionId { get; set; }
        [ForeignKey(nameof(SectionId))]
        public ReviewSection? Section { get; set; }
    }
}
