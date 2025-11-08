namespace TransnetAEX.Web.Models.Admin
{
    public class DashboardStatsViewModel
    {
        public int TotalReviewComplete { get; set; }
        public int TotalReviewIncomplete { get; set; }
        public int TotalReviewPending { get; set; }
        public int TotalUsers { get; set; }

        // Top 5 Reviews
        public List<string> TopReviewsNames { get; set; } = new();
        public List<int> TopReviewsCounts { get; set; } = new();

        // Top 5 Users
        public List<string> TopUsersNames { get; set; } = new();
        public List<int> TopUsersCounts { get; set; } = new();

        // Reviews by Status
        public List<string> StatusLabels { get; set; } = new();
        public List<int> StatusCounts { get; set; } = new();
    }
}
