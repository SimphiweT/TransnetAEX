namespace TransnetAEX.Web.Models.Reviews
{
    public class ReviewStageViewModel
    {
        public string StepName { get; set; } = string.Empty;
        public string Reviewer { get; set; } = string.Empty;
        public bool IsSignedOff { get; set; }
        public DateTime? SignedDate { get; set; }
        public string Comments { get; set; } = string.Empty;
    }
}
