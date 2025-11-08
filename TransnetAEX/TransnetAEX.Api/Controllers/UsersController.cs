using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TransnetAEX.Api.DTOs.Auth;
using TransnetAEX.Api.DTOs.Shared;
using TransnetAEX.Domain.Entities;

namespace TransnetAEX.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,HR")]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<SystemUser> userManager;
        private readonly IWebHostEnvironment env;

        public UsersController(UserManager<SystemUser> _userManager, IWebHostEnvironment _env)
        {
            userManager = _userManager;
            env = _env;
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
        {
            var users = userManager.Users.ToList();
            var list = new List<UserResponseDTO>();

            foreach (var u in users)
            {
                list.Add(new UserResponseDTO
                {
                    UserId = u.Id,
                    UsernameDisplay = u.DisplayName,
                    UserEmail = u.Email,
                    ContactNumber = u.ContactNumber,
                    Description = u.Description,
                    UserRole = "User",
                    ProfileImageFileName = $"{Request.Scheme}://{Request.Host}/images/users/{u.ProfilePicture ?? "default-user.png"}"
                });
            }

            return Ok(ApiResponseDTO<List<UserResponseDTO>>.SuccessResponse(list, "Users retrieved successfully."));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var user = await userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound(ApiResponseDTO<string>.FailResponse("User not found."));

            var dto = new UserResponseDTO
            {
                UserId = user.Id,
                UsernameDisplay = user.DisplayName,
                UserEmail = user.Email,
                Description = user.Description,
                ContactNumber = user.ContactNumber,
                ProfileImageFileName = $"{Request.Scheme}://{Request.Host}/images/users/{user.ProfilePicture ?? "default-user.png"}"
            };

            return Ok(ApiResponseDTO<UserResponseDTO>.SuccessResponse(dto, "User found."));
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromForm] CreateUserRequestDTO dto)
        {
            var user = new SystemUser
            {
                DisplayName = dto.UsernameDisplay,
                Email = dto.UserEmail,
                UserName = dto.UserEmail,
                Description = dto.Description,
                ContactNumber = dto.ContactNumber
            };

            var result = await userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                return BadRequest(ApiResponseDTO<string>.FailResponse("Registration failed.", string.Join(", ", result.Errors.Select(e => e.Description))));

            return Ok(ApiResponseDTO<UserResponseDTO>.SuccessResponse(new UserResponseDTO
            {
                UserId = user.Id,
                UsernameDisplay = user.DisplayName,
                UserEmail = user.Email
            }, "User registered successfully."));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromForm] UpdateUserRequestDTO dto)
        {
            var user = await userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound(ApiResponseDTO<string>.FailResponse("User not found."));

            user.DisplayName = dto.UsernameDisplay ?? user.DisplayName;
            user.Email = dto.UserEmail ?? user.Email;
            user.ContactNumber = dto.ContactNumber ?? user.ContactNumber;
            user.Description = dto.Description ?? user.Description;
            user.UpdatedDate = DateTime.UtcNow;

            var result = await userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return BadRequest(ApiResponseDTO<string>.FailResponse("Failed to update user."));

            return Ok(ApiResponseDTO<string>.SuccessResponse(null, "User updated successfully."));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound(ApiResponseDTO<string>.FailResponse("User not found."));

            await userManager.DeleteAsync(user);
            return Ok(ApiResponseDTO<string>.SuccessResponse(null, "User deleted successfully."));
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDTO dto)
        {
            var user = await userManager.FindByIdAsync(dto.UserId);
            if (user == null)
                return NotFound(ApiResponseDTO<string>.FailResponse("User not found."));

            var result = await userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
            if (!result.Succeeded)
                return BadRequest(ApiResponseDTO<string>.FailResponse("Password change failed.", string.Join(", ", result.Errors.Select(e => e.Description))));

            return Ok(ApiResponseDTO<string>.SuccessResponse(null, "Password changed successfully."));
        }

        [HttpPost("admin-reset-password")]
        public async Task<IActionResult> AdminResetPassword([FromForm] string email, [FromForm] string temporaryPassword)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
                return NotFound(ApiResponseDTO<string>.FailResponse("User not found."));

            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            var result = await userManager.ResetPasswordAsync(user, token, temporaryPassword);
            if (!result.Succeeded)
                return BadRequest(ApiResponseDTO<string>.FailResponse("Failed to reset password."));

            return Ok(ApiResponseDTO<string>.SuccessResponse(null, $"Temporary password set for {email}."));
        }
    }
}
