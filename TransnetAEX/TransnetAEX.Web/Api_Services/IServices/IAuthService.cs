using TransnetAEX.Web.Models.Auth;
using TransnetAEX.Web.Models.Shared;

namespace TransnetAEX.Web.Api_Services.IServices
{
    public interface IAuthService
    {
        Task<ApiResponseViewModel<LoginResponseViewModel>> LoginAsync(LoginRequestViewModel login);
        Task<ApiResponseViewModel<string>> ResetPasswordAsync(ResetPasswordViewModel resetPassword);
        Task<ApiResponseViewModel<string>> ForgotPasswordAsync(ForgotPasswordViewModel forgotPassword);
        Task<ApiResponseViewModel<string>> RequestAccessAsync(SignUpViewModel signUp);
        Task<ApiResponseViewModel<string>> LogoutAsync();
    }
}
