using Microsoft.EntityFrameworkCore;
using TransnetAEX.Domain.Entities;
using TransnetAEX.Domain.IRepositories;
using TransnetAEX.Infrastructure.Data;
using TransnetAEX.Infrastructure.EF_Repositories.Base;

namespace TransnetAEX.Infrastructure.EF_Repositories
{
    public class ReviewSectionRepository : RepositoryBase<ReviewSection>, IReviewSectionRepository
    {
        public ReviewSectionRepository(AppDbContext dbContext) : base(dbContext) { }

        public async Task<List<ReviewSection>> GetByReviewIdAsync(int reviewId)
        {
            return await appDbContext.ReviewSections
                .Include(s => s.Questions)
                .Where(s => s.ReviewId == reviewId)
                .ToListAsync();
        }
    }
}
