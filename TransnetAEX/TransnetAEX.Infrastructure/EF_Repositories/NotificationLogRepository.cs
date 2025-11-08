using TransnetAEX.Domain.Entities;
using TransnetAEX.Domain.IRepositories;
using TransnetAEX.Infrastructure.Data;
using TransnetAEX.Infrastructure.EF_Repositories.Base;

namespace TransnetAEX.Infrastructure.EF_Repositories
{
    public class NotificationLogRepository : RepositoryBase<NotificationLog>, INotificationLogRepository
    {
        public NotificationLogRepository(AppDbContext dbContext) : base(dbContext) { }
    }
}
