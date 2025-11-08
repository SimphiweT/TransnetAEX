using Microsoft.EntityFrameworkCore;
using TransnetAEX.Domain.Entities;
using TransnetAEX.Domain.IRepositories;
using TransnetAEX.Infrastructure.Data;
using TransnetAEX.Infrastructure.EF_Repositories.Base;

namespace TransnetAEX.Infrastructure.EF_Repositories
{
    public class HRInitiatedReviewRepository : RepositoryBase<HRInitiatedReview>, IHRInitiatedReviewRepository
    {
        public HRInitiatedReviewRepository(AppDbContext dbContext) : base(dbContext) { }

        public async Task<HRInitiatedReview?> GetFullReviewAsync(int id)
        {
            return await appDbContext.Reviews
                .Include(r => r.ReviewSections)
                .Include(r => r.Notifications)
                .FirstOrDefaultAsync(r => r.Id == id);
        }
    }
}
