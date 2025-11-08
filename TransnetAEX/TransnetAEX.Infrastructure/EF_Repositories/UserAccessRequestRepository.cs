using Microsoft.EntityFrameworkCore;
using TransnetAEX.Domain.Entities;
using TransnetAEX.Domain.IRepositories;
using TransnetAEX.Infrastructure.Data;
using TransnetAEX.Infrastructure.EF_Repositories.Base;

namespace TransnetAEX.Infrastructure.EF_Repositories
{
    /// <summary>
    /// EF Core repository implementation for managing access request data.
    /// </summary>
    public class UserAccessRequestRepository : RepositoryBase<UserAccessRequest>, IUserAccessRequestRepository
    {
        public UserAccessRequestRepository(AppDbContext appDbContext) : base(appDbContext) { }

        public async Task<IEnumerable<UserAccessRequest>> GetPendingRequestsAsync()
        {
            return await appDbContext.UserAccessRequests
                .AsNoTracking()
                .Where(r => !r.IsApproved && r.UpdatedAt == null)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<UserAccessRequest>> GetReviewedRequestsAsync()
        {
            return await appDbContext.UserAccessRequests
                .AsNoTracking()
                .Where(r => r.UpdatedAt != null)
                .OrderByDescending(r => r.UpdatedAt)
                .ToListAsync();
        }
    }
}
