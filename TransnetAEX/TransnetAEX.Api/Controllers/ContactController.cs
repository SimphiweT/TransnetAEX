using Microsoft.AspNetCore.Mvc;
using TransnetAEX.Api.DTOs.ContactDTOs;
using TransnetAEX.Api.DTOs.Shared;
using TransnetAEX.Application.Helpers;
using TransnetAEX.Application.Interfaces;

namespace TransnetAEX.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContactController : ControllerBase
    {
        private readonly IEmailService _emailService;
        private readonly IConfiguration _config;

        public ContactController(IEmailService emailService, IConfiguration config)
        {
            _emailService = emailService;
            _config = config;
        }

        [HttpPost("send")]
        public async Task<IActionResult> Send([FromBody] ContactRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponseDTO<string>.FailResponse("Invalid request data."));

            try
            {
                string logoUrl = $"{Request.Scheme}://{Request.Host}/images/transnet_logo.png";

                string body = $@"
                    <p><strong>Name:</strong> {request.Name}</p>
                    <p><strong>Email:</strong> {request.Email}</p>
                    <p><strong>Message:</strong><br/>{request.Message}</p>";

                string htmlTemplate = EmailTemplateHelper.BuildTemplate(
                    logoUrl: logoUrl,
                    title: "New Message - Transnet Exit Reviews System (AEX)",
                    body: body
                );

                // ✅ FIXED: parameter name now matches your service method
                await _emailService.SendEmailAsync(
                    toEmail: "hrsupport@transnet.net",
                    subject: $"AEX Contact Form - {request.Name}",
                    htmlBody: htmlTemplate
                );

                return Ok(ApiResponseDTO<string>.SuccessResponse(null, "Message sent successfully to HR Support."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseDTO<string>.FailResponse(
                    $"Failed to send email: {ex.Message}",
                    "Email sending failed."
                ));
            }
        }
    }
}
