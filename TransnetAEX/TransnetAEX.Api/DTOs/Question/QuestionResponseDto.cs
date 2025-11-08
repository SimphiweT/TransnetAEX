namespace TransnetAEX.Api.DTOs.Question
{
    public class QuestionResponseDto
    {
        public int QuestionId { get; set; }
        public string SectionType { get; set; } = string.Empty;
        public string QuestionText { get; set; } = string.Empty;
        public string InputType { get; set; } = string.Empty;
        public string? Options { get; set; }
        public bool IsRequired { get; set; }
        public bool IsActive { get; set; }
    }
}
