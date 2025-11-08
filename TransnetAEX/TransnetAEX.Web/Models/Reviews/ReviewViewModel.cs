namespace TransnetAEX.Web.Models.Reviews
{
    public class ReviewViewModel
    {
        public string ReviewId { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending";
        public DateTime SubmittedDate { get; set; }
        public List<ReviewStageViewModel> Stages { get; set; } = new();
    }
}
