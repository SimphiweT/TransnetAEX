using Microsoft.AspNetCore.Mvc;
using TransnetAEX.Application.Interfaces;
using TransnetAEX.Application.Helpers;

namespace TransnetAEX.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmailController : ControllerBase
    {
        private readonly IEmailService _email;

        public EmailController(IEmailService email)
        {
            _email = email;
        }

        [HttpPost("test")]
        public async Task<IActionResult> SendTestEmail([FromQuery] string to)
        {
            if (string.IsNullOrWhiteSpace(to))
                return BadRequest("Recipient email address is required.");

            string htmlBody = EmailTemplateHelper.BuildTemplate(
                "https://upload.wikimedia.org/wikipedia/commons/4/4f/Transnet_logo.svg",
                "SMTP Test Email",
                "<p>This is a test email from <strong>Transnet AEX API</strong>. 🎉</p>"
            );

            try
            {
                await _email.SendEmailAsync(to, "✅ SMTP Test - Transnet AEX API", htmlBody);
                return Ok(new { success = true, message = $"Email successfully sent to {to}" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }
    }
}
