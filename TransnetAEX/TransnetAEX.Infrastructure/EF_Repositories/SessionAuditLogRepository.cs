using Microsoft.EntityFrameworkCore;
using TransnetAEX.Domain.Entities;
using TransnetAEX.Domain.IRepositories;
using TransnetAEX.Infrastructure.Data;
using TransnetAEX.Infrastructure.EF_Repositories.Base;

namespace TransnetAEX.Infrastructure.EF_Repositories
{
    public class SessionAuditLogRepository : RepositoryBase<SessionAuditLog>, ISessionAuditLogRepository
    {
        public SessionAuditLogRepository(AppDbContext dbContext) : base(dbContext) { }

        public async Task<IEnumerable<SessionAuditLog>> GetByUserIdAsync(string userId)
        {
            return await appDbContext.SessionAuditLogs
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.Timestamp)
                .ToListAsync();
        }
    }
}
