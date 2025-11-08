using TransnetAEX.Domain.Entities;
using TransnetAEX.Domain.IRepositories.Base;

namespace TransnetAEX.Domain.IRepositories
{
    public interface IReviewSectionRepository : IRepositoryBase<ReviewSection>
    {
        Task<List<ReviewSection>> GetByReviewIdAsync(int reviewId);
    }
}
