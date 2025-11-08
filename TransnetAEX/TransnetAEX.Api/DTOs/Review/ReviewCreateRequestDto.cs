namespace TransnetAEX.Api.DTOs.Review
{
    public class ReviewCreateRequestDto
    {
        public string HrEmail { get; set; } = string.Empty;
        public string EmployeeEmail { get; set; } = string.Empty;
        public string LineManagerEmail { get; set; } = string.Empty;
    }
}
