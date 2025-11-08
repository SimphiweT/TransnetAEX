
using System.Linq.Expressions;

namespace TransnetAEX.Domain.IRepositories.Base
{
    public interface IRepositoryWrapper
    {
        IHRInitiatedReviewRepository InitiatedReview { get; }
        INotificationLogRepository Notification { get; }
        IQuestionBankRepository Question { get; }
        IReviewQuestionRepository ReviewQuestion { get; }
        IReviewSectionRepository ReviewSection { get; }
        ISystemUserRepository User { get; }
        ISystemUserRolesRepository UserRoles { get; }
        ISessionAuditLogRepository SessionAuditLog { get; }
        IUserAccessRequestRepository UserAccessRequest { get; }
        Task SaveAsync();
    }
}
