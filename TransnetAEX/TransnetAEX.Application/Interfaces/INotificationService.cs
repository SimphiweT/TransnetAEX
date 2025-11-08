using TransnetAEX.Domain.Entities;

namespace TransnetAEX.Application.Interfaces
{
    public interface INotificationService
    {
        Task<IEnumerable<NotificationLog>> GetNotificationsAsync();
        Task<IEnumerable<NotificationLog>> GetNotificationsByReviewAsync(int reviewId);
        Task LogNotificationAsync(string recipientEmail, string type);
    }
}
