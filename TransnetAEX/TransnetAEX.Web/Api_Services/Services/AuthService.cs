using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;
using TransnetAEX.Web.Api_Services.IServices;
using TransnetAEX.Web.Helpers;
using TransnetAEX.Web.Models.Auth;
using TransnetAEX.Web.Models.Shared;

namespace TransnetAEX.Web.Api_Services.Services
{
    public class AuthService : IAuthService
    {
        private readonly HttpClient client;
        private readonly ApiSettings apiSettings;
        private readonly SessionManager sessionManager;
        private readonly string apiBaseUrl;

        public AuthService(HttpClient _client, IOptions<ApiSettings> _apiSettings, SessionManager _sessionManager)
        {
            client = _client;
            apiSettings = _apiSettings.Value;
            sessionManager = _sessionManager;
            apiBaseUrl = apiSettings.BaseUrl + "auth";
        }

        public async Task<ApiResponseViewModel<LoginResponseViewModel>> LoginAsync(LoginRequestViewModel login)
        {
            try
            {
                var content = new StringContent(JsonConvert.SerializeObject(login), Encoding.UTF8, "application/json");
                var response = await client.PostAsync($"{apiBaseUrl}/login", content);

                var responseBody = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                    return new ApiResponseViewModel<LoginResponseViewModel> { Success = false, Error = responseBody };

                var result = JsonConvert.DeserializeObject<ApiResponseViewModel<LoginResponseViewModel>>(responseBody);
                if (result?.Data?.Token != null)
                {
                    sessionManager.SetSession(result.Data.Token, result.Data.Role, result.Data.UserId,
                        result.Data.DisplayName, result.Data.Email, result.Data.ProfileImageUrl);
                }
                return result ?? new ApiResponseViewModel<LoginResponseViewModel> { Success = false, Error = "Invalid response format." };
            }
            catch (Exception ex)
            {
                return new ApiResponseViewModel<LoginResponseViewModel> { Success = false, Error = ex.Message };
            }
        }

        public async Task<ApiResponseViewModel<string>> ForgotPasswordAsync(ForgotPasswordViewModel forgotPassword)
        {
            try
            {
                var content = new StringContent(JsonConvert.SerializeObject(forgotPassword), Encoding.UTF8, "application/json");
                var response = await client.PostAsync($"{apiBaseUrl}/forgot-password", content);

                var result = JsonConvert.DeserializeObject<ApiResponseViewModel<string>>(await response.Content.ReadAsStringAsync());
                return result ?? new ApiResponseViewModel<string> { Success = false, Error = "Invalid response format." };
            }
            catch (Exception ex)
            {
                return new ApiResponseViewModel<string> { Success = false, Error = ex.Message };
            }
        }

        public async Task<ApiResponseViewModel<string>> ResetPasswordAsync(ResetPasswordViewModel resetPassword)
        {
            try
            {
                var content = new StringContent(JsonConvert.SerializeObject(resetPassword), Encoding.UTF8, "application/json");
                var response = await client.PostAsync($"{apiBaseUrl}/reset-password", content);

                var result = JsonConvert.DeserializeObject<ApiResponseViewModel<string>>(await response.Content.ReadAsStringAsync());
                return result ?? new ApiResponseViewModel<string> { Success = false, Error = "Invalid response format." };
            }
            catch (Exception ex)
            {
                return new ApiResponseViewModel<string> { Success = false, Error = ex.Message };
            }
        }

        public async Task<ApiResponseViewModel<string>> RequestAccessAsync(SignUpViewModel signUp)
        {
            try
            {
                using var form = new MultipartFormDataContent();
                form.Add(new StringContent(signUp.DisplayName), "DisplayName");
                form.Add(new StringContent(signUp.Email), "Email");
                form.Add(new StringContent(signUp.Role), "RoleApplied");
                if (!string.IsNullOrWhiteSpace(signUp.Message))
                    form.Add(new StringContent(signUp.Message), "Message");
                if (signUp.Document != null)
                {
                    var stream = signUp.Document.OpenReadStream();
                    form.Add(new StreamContent(stream), "PdfDocument", signUp.Document.FileName);
                }

                var response = await client.PostAsync($"{apiBaseUrl}/request-access", form);
                var result = JsonConvert.DeserializeObject<ApiResponseViewModel<string>>(await response.Content.ReadAsStringAsync());
                return result ?? new ApiResponseViewModel<string> { Success = false, Error = "Invalid response format." };
            }
            catch (Exception ex)
            {
                return new ApiResponseViewModel<string> { Success = false, Error = ex.Message };
            }
        }

        public async Task<ApiResponseViewModel<string>> LogoutAsync()
        {
            try
            {
                var token = sessionManager.GetToken();
                if (string.IsNullOrEmpty(token))
                    return new ApiResponseViewModel<string> { Success = false, Error = "No active session." };

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var response = await client.PostAsync($"{apiBaseUrl}/logout", null);
                sessionManager.Clear();

                if (response.IsSuccessStatusCode)
                    return new ApiResponseViewModel<string> { Success = true, Message = "Logout successful." };
                else
                    return new ApiResponseViewModel<string> { Success = false, Error = "Logout failed." };
            }
            catch (Exception ex)
            {
                return new ApiResponseViewModel<string> { Success = false, Error = ex.Message };
            }
        }
    }
}
