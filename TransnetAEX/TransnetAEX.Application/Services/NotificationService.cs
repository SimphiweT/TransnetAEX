using TransnetAEX.Application.Interfaces;
using TransnetAEX.Domain.Entities;
using TransnetAEX.Domain.IRepositories.Base;

namespace TransnetAEX.Application.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IRepositoryWrapper _repo;

        public NotificationService(IRepositoryWrapper repo)
        {
            _repo = repo;
        }

        public async Task<IEnumerable<NotificationLog>> GetNotificationsAsync()
        {
            return await _repo.Notification.GetAllAsync();
        }

        public async Task<IEnumerable<NotificationLog>> GetNotificationsByReviewAsync(int reviewId)
        {
            var notifications = await _repo.Notification.GetAllByCondition(n => n.ReviewId == reviewId);
            return notifications.OrderByDescending(n => n.CreatedDate);
        }

        public async Task LogNotificationAsync(string recipientEmail, string type)
        {
            var log = new NotificationLog
            {
                RecipientEmail = recipientEmail,
                NotificationType = type,
                Status = "Sent",
                CreatedDate = DateTime.UtcNow
            };

            await _repo.Notification.CreateAsync(log);
            await _repo.SaveAsync();
        }
    }
}
