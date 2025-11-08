using TransnetAEX.Web.Models.Auth;
using TransnetAEX.Web.Models.Shared;

namespace TransnetAEX.Web.Api_Services.IServices
{
    public interface IUserService
    {
        Task<ApiResponseViewModel<List<UserViewModel>>> GetAllUsersAsync();
        Task<ApiResponseViewModel<UserViewModel>> GetUserByIdAsync(string userId);
        Task<ApiResponseViewModel<UserViewModel>> RegisterUserAsync(CreateUserViewModel newUser);
        Task<ApiResponseViewModel<UserViewModel>> UpdateUserAsync(UpdateUserViewModel updatedUser);
        Task<ApiResponseViewModel<string>> ChangePasswordAsync(ChangePasswordViewModel changePassword);
        Task<ApiResponseViewModel<string>> AdminResetPasswordAsync(string email, string temporaryPassword);
        Task<ApiResponseViewModel<string>> DeleteUserAsync(string userId);
    }
}
