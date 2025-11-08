using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TransnetAEX.Api.DTOs.Auth;
using TransnetAEX.Api.DTOs.Shared;
using TransnetAEX.Application.Interfaces;
using TransnetAEX.Domain.Entities;
using TransnetAEX.Domain.IRepositories.Base;

namespace TransnetAEX.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccessController : ControllerBase
    {
        private readonly IRepositoryWrapper repo;
        private readonly IEmailService emailService;
        private readonly IWebHostEnvironment env;
        private readonly IConfiguration config;

        public AccessController(
            IRepositoryWrapper _repo,
            IEmailService _emailService,
            IWebHostEnvironment _env,
            IConfiguration _config)
        {
            repo = _repo;
            emailService = _emailService;
            env = _env;
            config = _config;
        }

        // ==========================================================
        // 🧾 SUBMIT ACCESS REQUEST
        // ==========================================================
        [HttpPost("request")]
        [AllowAnonymous]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> RequestAccess([FromForm] AccessRequestDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponseDTO<string>.FailResponse("Invalid request form."));

            var existing = await repo.UserAccessRequest.GetAllByCondition(u => u.Email == dto.Email);
            if (existing.Any())
                return Conflict(ApiResponseDTO<string>.FailResponse("Access request already submitted."));

            var newRequest = new UserAccessRequest
            {
                DisplayName = dto.DisplayName,
                Email = dto.Email,
                RoleApplied = dto.RoleApplied,
                Message = dto.Message,
                PdfFilePath = dto.PdfDocument?.FileName,
                CreatedAt = DateTime.UtcNow
            };

            await repo.UserAccessRequest.CreateAsync(newRequest);
            await repo.SaveAsync();

            // Optionally email admins
            try
            {
                var logo = $"{Request.Scheme}://{Request.Host}/images/logos/transnet_logo.png";
                string body = $"<p><strong>{dto.DisplayName}</strong> requested access for role <b>{dto.RoleApplied}</b>.</p><p>Email: {dto.Email}</p>";
                string html = $"<html><body><img src='{logo}' width='150'/><br/><br/>{body}</body></html>";

                var admins = config["AppSettings:AdminNotifyEmails"]?.Split(',') ?? Array.Empty<string>();
                foreach (var email in admins)
                    await emailService.SendEmailAsync(email, "New Access Request", html);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Email error: {ex.Message}");
            }

            return Ok(ApiResponseDTO<string>.SuccessResponse(null, "Access request submitted successfully."));
        }

        // ==========================================================
        // 📋 GET ALL REQUESTS
        // ==========================================================
        [HttpGet("all")]
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> GetAll()
        {
            var list = await repo.UserAccessRequest.GetAllAsync();
            return Ok(ApiResponseDTO<IEnumerable<UserAccessRequest>>.SuccessResponse(list, "Access requests retrieved."));
        }

        // ==========================================================
        // ✅ APPROVE REQUEST
        // ==========================================================
        [HttpPost("{id}/approve")]
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> Approve(int id)
        {
            var request = await repo.UserAccessRequest.GetByIdAsync(id);
            if (request == null)
                return NotFound(ApiResponseDTO<string>.FailResponse("Request not found."));

            request.IsApproved = true;
            request.ApprovedBy = User?.Identity?.Name ?? "System";
            request.UpdatedAt = DateTime.UtcNow;
            request.DeclinedBy = null;

            await repo.SaveAsync();

            return Ok(ApiResponseDTO<string>.SuccessResponse(null, "Access request approved successfully."));
        }

        // ==========================================================
        // ❌ DECLINE REQUEST
        // ==========================================================
        [HttpPost("{id}/decline")]
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> Decline(int id, [FromBody] dynamic data)
        {
            var reason = (string?)data?.reason ?? "No reason provided.";
            var request = await repo.UserAccessRequest.GetByIdAsync(id);
            if (request == null)
                return NotFound(ApiResponseDTO<string>.FailResponse("Request not found."));

            request.IsApproved = false;
            request.DeclinedBy = User?.Identity?.Name ?? "System";
            request.Message = $"{request.Message}\nDecline reason: {reason}";
            request.UpdatedAt = DateTime.UtcNow;

            await repo.SaveAsync();

            return Ok(ApiResponseDTO<string>.SuccessResponse(null, "Access request declined successfully."));
        }
    }
}
