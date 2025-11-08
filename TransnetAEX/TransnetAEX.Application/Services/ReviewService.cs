using TransnetAEX.Application.Helpers;
using TransnetAEX.Application.Interfaces;
using TransnetAEX.Domain.Entities;
using TransnetAEX.Domain.IRepositories.Base;

namespace TransnetAEX.Application.Services
{
    public class ReviewService : IReviewService
    {
        private readonly IRepositoryWrapper _repo;
        private readonly IEmailService _email;

        public ReviewService(IRepositoryWrapper repo, IEmailService email)
        {
            _repo = repo;
            _email = email;
        }

        // ===============================================================
        // 1️⃣ Get All / Single Review
        // ===============================================================
        public async Task<IEnumerable<HRInitiatedReview>> GetAllReviewsAsync()
        {
            var reviews = await _repo.InitiatedReview.GetAllAsync();
            return reviews.OrderByDescending(r => r.CreatedDate);
        }

        public async Task<HRInitiatedReview?> GetReviewByIdAsync(int id)
        {
            return await _repo.InitiatedReview.GetFullReviewAsync(id);
        }

        // ===============================================================
        // 2️⃣ HR Initiates Review → Employee notified
        // ===============================================================
        public async Task<HRInitiatedReview> InitiateReviewAsync(string hrEmail, string employeeEmail, string lineManagerEmail)
        {
            var review = new HRInitiatedReview
            {
                HrEmail = hrEmail,
                EmployeeEmail = employeeEmail,
                LineManagerEmail = lineManagerEmail,
                Status = "Pending Employee",
                CreatedDate = DateTime.UtcNow
            };

            // Create Sections
            review.ReviewSections.Add(new ReviewSection
            {
                SectionType = "Employee",
                ReviewerEmail = employeeEmail,
                CreatedDate = DateTime.UtcNow
            });

            review.ReviewSections.Add(new ReviewSection
            {
                SectionType = "LineManager",
                ReviewerEmail = lineManagerEmail,
                CreatedDate = DateTime.UtcNow
            });

            await _repo.InitiatedReview.CreateAsync(review);
            await _repo.SaveAsync();

            // Send Employee Email
            var employeeBody = EmailTemplateHelper.BuildTemplate(
                "https://upload.wikimedia.org/wikipedia/commons/4/4f/Transnet_logo.svg",
                "Exit Review Form - Employee Section",
                $@"<p>Dear Employee,</p>
                   <p>Please complete your exit review form by clicking the link below:</p>
                   <p><a href='http://localhost:5055/Review/Employee/{review.Id}' 
                         style='color:#d71920;'>Click here to complete your review</a></p>");

            await _email.SendEmailAsync(employeeEmail,
                "Transnet Exit Review - Employee Action Required", employeeBody);

            // Log Notification
            review.Notifications.Add(new NotificationLog
            {
                RecipientEmail = employeeEmail,
                NotificationType = "EmployeeLink",
                Status = "Sent",
                CreatedDate = DateTime.UtcNow
            });
            await _repo.SaveAsync();

            return review;
        }

        // ===============================================================
        // 3️⃣ Employee completes → Notify Line Manager
        // ===============================================================
        public async Task<bool> EmployeeSubmitAsync(int reviewId)
        {
            var review = await _repo.InitiatedReview.GetByIdAsync(reviewId);
            if (review == null) return false;

            review.Status = "Pending Manager";
            review.UpdatedDate = DateTime.UtcNow;

            await _repo.InitiatedReview.UpdateAsync(review);
            await _repo.SaveAsync();

            // Send Line Manager Email
            var managerBody = EmailTemplateHelper.BuildTemplate(
                "https://upload.wikimedia.org/wikipedia/commons/4/4f/Transnet_logo.svg",
                "Exit Review Form - Line Manager Section",
                $@"<p>Dear Line Manager,</p>
                   <p>The employee has completed their section of the exit review form. 
                   Please complete your section by clicking the link below:</p>
                   <p><a href='http://localhost:5055/Review/Manager/{review.Id}'
                         style='color:#d71920;'>Open Manager Review Form</a></p>");

            await _email.SendEmailAsync(review.LineManagerEmail,
                "Transnet Exit Review - Manager Action Required", managerBody);

            // Log Notification
            review.Notifications.Add(new NotificationLog
            {
                RecipientEmail = review.LineManagerEmail,
                NotificationType = "ManagerLink",
                Status = "Sent",
                CreatedDate = DateTime.UtcNow
            });
            await _repo.SaveAsync();

            return true;
        }

        // ===============================================================
        // 4️⃣ Line Manager completes → Notify HR (Final)
        // ===============================================================
        public async Task<bool> ManagerSubmitAsync(int reviewId)
        {
            var review = await _repo.InitiatedReview.GetByIdAsync(reviewId);
            if (review == null) return false;

            review.Status = "Completed";
            review.UpdatedDate = DateTime.UtcNow;
            await _repo.InitiatedReview.UpdateAsync(review);
            await _repo.SaveAsync();

            // Notify HR of completion
            var hrBody = EmailTemplateHelper.BuildTemplate(
                "https://upload.wikimedia.org/wikipedia/commons/4/4f/Transnet_logo.svg",
                "Exit Review Completed",
                $@"<p>Dear HR,</p>
                   <p>The exit review process for <strong>{review.EmployeeEmail}</strong> has been completed.</p>
                   <p>You can now log in to the system to review and archive the record.</p>");

            await _email.SendEmailAsync(review.HrEmail, "Transnet Exit Review Completed", hrBody);

            // Log Notification
            review.Notifications.Add(new NotificationLog
            {
                RecipientEmail = review.HrEmail,
                NotificationType = "Completion",
                Status = "Sent",
                CreatedDate = DateTime.UtcNow
            });
            await _repo.SaveAsync();

            return true;
        }

        // ===============================================================
        // 5️⃣ Manual Marking (Admin use)
        // ===============================================================
        public async Task<bool> MarkReviewCompletedAsync(int id)
        {
            var review = await _repo.InitiatedReview.GetByIdAsync(id);
            if (review == null) return false;

            review.Status = "Completed";
            review.UpdatedDate = DateTime.UtcNow;

            await _repo.InitiatedReview.UpdateAsync(review);
            await _repo.SaveAsync();

            return true;
        }
    }
}
