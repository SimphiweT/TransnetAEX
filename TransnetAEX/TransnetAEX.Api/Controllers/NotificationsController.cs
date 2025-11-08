using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TransnetAEX.Application.Helpers;
using TransnetAEX.Application.Interfaces;

namespace TransnetAEX.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationsController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        // -------------------- GET: api/notifications --------------------
        [HttpGet]
        //[Authorize(Roles = "Admin,HR")]
        [Authorize(Roles = $"{ConstantsUtils.ADMIN_ROLE},{ConstantsUtils.HR_ROLE}")]
        public async Task<IActionResult> GetAllNotifications()
        {
            var logs = await _notificationService.GetNotificationsAsync();
            return Ok(logs);
        }

        // -------------------- GET: api/notifications/review/{reviewId} --------------------
        [HttpGet("review/{reviewId:int}")]
        //[Authorize(Roles = "Admin,HR,LineManager,Employee")]
        [Authorize(Roles = $"{ConstantsUtils.ADMIN_ROLE},{ConstantsUtils.HR_ROLE}")]
        public async Task<IActionResult> GetNotificationsByReview(int reviewId)
        {
            var logs = await _notificationService.GetNotificationsByReviewAsync(reviewId);
            return Ok(logs);
        }

        // -------------------- POST: api/notifications --------------------
        [HttpPost]
        //[Authorize(Roles = "Admin,HR")]
        [Authorize(Roles = $"{ConstantsUtils.ADMIN_ROLE},{ConstantsUtils.HR_ROLE}")]
        public async Task<IActionResult> LogNotification(string recipientEmail, string type)
        {
            await _notificationService.LogNotificationAsync(recipientEmail, type);
            return Ok(new { Message = "Notification logged successfully." });
        }
    }
}
