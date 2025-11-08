namespace TransnetAEX.Api.DTOs.Question
{
    public class QuestionCreateRequestDto
    {
        public string SectionType { get; set; } = string.Empty;  // Employee / LineManager
        public string QuestionText { get; set; } = string.Empty;
        public string InputType { get; set; } = "Text";
        public string? Options { get; set; }
        public bool IsRequired { get; set; } = false;
    }
}
