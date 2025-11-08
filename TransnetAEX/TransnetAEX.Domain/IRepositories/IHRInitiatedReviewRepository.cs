using TransnetAEX.Domain.Entities;
using TransnetAEX.Domain.IRepositories.Base;

namespace TransnetAEX.Domain.IRepositories
{
    public interface IHRInitiatedReviewRepository : IRepositoryBase<HRInitiatedReview>
    {
        Task<HRInitiatedReview?> GetFullReviewAsync(int id); // include sections & notifications
    }
}
