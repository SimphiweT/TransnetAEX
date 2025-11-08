using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TransnetAEX.Api.DTOs.Auth;
using TransnetAEX.Api.DTOs.Shared;
using TransnetAEX.Application.Helpers;
using TransnetAEX.Application.Interfaces;
using TransnetAEX.Domain.Entities;
using TransnetAEX.Domain.IRepositories.Base;

namespace TransnetAEX.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IRepositoryWrapper repo;
        private readonly IEmailService emailService;
        private readonly UserManager<SystemUser> userManager;
        private readonly IConfiguration config;
        private readonly IWebHostEnvironment env;

        public AuthController(
            IRepositoryWrapper _repo,
            IEmailService _emailService,
            UserManager<SystemUser> _userManager,
            IConfiguration _config,
            IWebHostEnvironment _env)
        {
            repo = _repo;
            emailService = _emailService;
            userManager = _userManager;
            config = _config;
            env = _env;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO dto)
        {
            var user = await userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                return Unauthorized(ApiResponseDTO<string>.FailResponse("Invalid credentials."));

            if (!await userManager.CheckPasswordAsync(user, dto.Password))
                return Unauthorized(ApiResponseDTO<string>.FailResponse("Invalid credentials."));

            var roles = await userManager.GetRolesAsync(user);
            var token = TokenHelper.GenerateJwtToken(user, roles, config, 6);
            var result = new LoginResponseDTO
            {
                UserId = user.Id,
                Email = user.Email!,
                DisplayName = user.DisplayName,
                Role = roles.FirstOrDefault() ?? "User",
                Token = token,
                TokenExpiry = DateTime.UtcNow.AddHours(6),
                ProfileImageUrl = $"{Request.Scheme}://{Request.Host}/images/users/{user.ProfilePicture ?? "default-user.png"}"
            };

            return Ok(ApiResponseDTO<LoginResponseDTO>.SuccessResponse(result, "Login successful."));
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var email = User?.Identity?.Name;
            if (email == null)
                return Unauthorized(ApiResponseDTO<string>.FailResponse("Invalid session."));

            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
                return NotFound(ApiResponseDTO<string>.FailResponse("User not found."));

            user.RefreshToken = null;
            user.RefreshTokenExpiry = null;
            await userManager.UpdateAsync(user);

            return Ok(ApiResponseDTO<string>.SuccessResponse(null, "Logout successful."));
        }

        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDTO dto)
        {
            var user = await userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                return Ok(ApiResponseDTO<string>.SuccessResponse(null, "If the email exists, a reset link will be sent."));

            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            var encoded = Uri.EscapeDataString(token);
            var resetLink = $"{config["AppSettings:PasswordResetUrl"]}?email={dto.Email}&token={encoded}";

            string html = EmailTemplateHelper.BuildTemplate(
                $"{Request.Scheme}://{Request.Host}/images/logos/transnet_logo.png",
                "Password Reset",
                $"<p>Hello {user.DisplayName},</p><p><a href='{resetLink}'>Click here to reset your password</a></p>"
            );

            await emailService.SendEmailAsync(user.Email!, "Transnet AEX Password Reset", html);

            return Ok(ApiResponseDTO<string>.SuccessResponse(null, "Password reset link sent successfully."));
        }

        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDTO dto)
        {
            var user = await userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                return BadRequest(ApiResponseDTO<string>.FailResponse("Invalid email or token."));

            var result = await userManager.ResetPasswordAsync(user, dto.Token, dto.NewPassword);
            if (!result.Succeeded)
                return BadRequest(ApiResponseDTO<string>.FailResponse("Failed to reset password.", string.Join(", ", result.Errors.Select(e => e.Description))));

            return Ok(ApiResponseDTO<string>.SuccessResponse(null, "Password reset successfully."));
        }
    }
}
