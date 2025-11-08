using TransnetAEX.Domain.Entities;

namespace TransnetAEX.Application.Interfaces
{
    public interface IReviewService
    {
        // Core Queries
        Task<IEnumerable<HRInitiatedReview>> GetAllReviewsAsync();
        Task<HRInitiatedReview?> GetReviewByIdAsync(int id);

        // Workflow Steps
        Task<HRInitiatedReview> InitiateReviewAsync(string hrEmail, string employeeEmail, string lineManagerEmail);
        Task<bool> EmployeeSubmitAsync(int reviewId);
        Task<bool> ManagerSubmitAsync(int reviewId);
        Task<bool> MarkReviewCompletedAsync(int id);
    }
}
