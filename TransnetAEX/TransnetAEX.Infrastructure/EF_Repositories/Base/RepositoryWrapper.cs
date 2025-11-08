using Microsoft.AspNetCore.Identity;
using TransnetAEX.Domain.Entities;
using TransnetAEX.Domain.IRepositories;
using TransnetAEX.Domain.IRepositories.Base;
using TransnetAEX.Infrastructure.Data;

namespace TransnetAEX.Infrastructure.EF_Repositories.Base
{
    public class RepositoryWrapper : IRepositoryWrapper
    {
        private readonly AppDbContext appDbContext;
        private readonly UserManager<SystemUser> userManager;
        private readonly RoleManager<SystemUserRoles> roleManager;

        public RepositoryWrapper(AppDbContext context, UserManager<SystemUser> userMgr, RoleManager<SystemUserRoles> roleMgr)
        {
            appDbContext = context;
            userManager = userMgr;
            roleManager = roleMgr;
        }

        private IHRInitiatedReviewRepository? initiatedReview;
        private INotificationLogRepository? notification;
        private IQuestionBankRepository? question;
        private IReviewQuestionRepository? reviewQuestion;
        private IReviewSectionRepository? reviewSection;
        private ISystemUserRepository? user;
        private ISystemUserRolesRepository? userRoles;
        private ISessionAuditLogRepository? sessionAuditLog;
        private IUserAccessRequestRepository? userAccessRequest;

        public IHRInitiatedReviewRepository InitiatedReview => initiatedReview ??= new HRInitiatedReviewRepository(appDbContext);
        public INotificationLogRepository Notification => notification ??= new NotificationLogRepository(appDbContext);
        public IQuestionBankRepository Question => question ??= new QuestionBankRepository(appDbContext);
        public IReviewQuestionRepository ReviewQuestion => reviewQuestion ??= new ReviewQuestionRepository(appDbContext);
        public IReviewSectionRepository ReviewSection => reviewSection ??= new ReviewSectionRepository(appDbContext);
        public ISystemUserRepository User => user ??= new SystemUserRepository(appDbContext, userManager);
        public ISystemUserRolesRepository UserRoles => userRoles ??= new SystemUserRolesRepository(appDbContext, roleManager);

        public ISessionAuditLogRepository SessionAuditLog => sessionAuditLog ??= new SessionAuditLogRepository(appDbContext);

        public IUserAccessRequestRepository UserAccessRequest
        {
            get
            {
                if (userAccessRequest == null)
                {
                    userAccessRequest = new UserAccessRequestRepository(appDbContext);
                }
                return userAccessRequest;
            }
        }

        public async Task SaveAsync() => await appDbContext.SaveChangesAsync();
    }
}
