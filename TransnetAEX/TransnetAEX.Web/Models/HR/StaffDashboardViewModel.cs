namespace TransnetAEX.Web.Models.HR
{
    public class StaffDashboardViewModel
    {
        public int TotalReviewComplete { get; set; }
        public int TotalReviewIncomplete { get; set; }
        public int TotalReviewPending { get; set; }
        public int TotaEmployeeQuestions { get; set; }
        public int TotaLineManagerQuestions { get; set; }

        // Top 5 Reviews
        public List<string> TopReviewsNames { get; set; } = new();
        public List<int> TopReviewsCounts { get; set; } = new();

        // Top 5 Questions from Employees
        public List<string> TopEmployeeQuestions { get; set; } = new();
        public List<int> TopEmployeeQuestionsCounts { get; set; } = new();

        // Reviews by Status
        public List<string> StatusLabels { get; set; } = new();
        public List<int> StatusCounts { get; set; } = new();
    }
}
