using TransnetAEX.Domain.Entities;
using TransnetAEX.Domain.IRepositories.Base;

namespace TransnetAEX.Domain.IRepositories
{
    public interface ISessionAuditLogRepository : IRepositoryBase<SessionAuditLog>
    {
        Task<IEnumerable<SessionAuditLog>> GetByUserIdAsync(string userId);
    }
}
