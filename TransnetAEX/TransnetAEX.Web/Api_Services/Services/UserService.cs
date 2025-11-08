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
    public class UserService : IUserService
    {
        private readonly HttpClient client;
        private readonly ApiSettings apiSettings;
        private readonly SessionManager sessionManager;
        private readonly string apiBaseUrl;

        public UserService(HttpClient _client, IOptions<ApiSettings> _apiSettings, SessionManager _sessionManager)
        {
            client = _client;
            apiSettings = _apiSettings.Value;
            sessionManager = _sessionManager;
            apiBaseUrl = apiSettings.BaseUrl + "users";
        }

        private void SetAuthHeader()
        {
            var token = sessionManager.GetToken();
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        public async Task<ApiResponseViewModel<List<UserViewModel>>> GetAllUsersAsync()
        {
            try
            {
                SetAuthHeader();
                var response = await client.GetAsync(apiBaseUrl);
                var result = JsonConvert.DeserializeObject<ApiResponseViewModel<List<UserViewModel>>>(await response.Content.ReadAsStringAsync());
                return result ?? new ApiResponseViewModel<List<UserViewModel>> { Success = false, Error = "Invalid response format." };
            }
            catch (Exception ex)
            {
                return new ApiResponseViewModel<List<UserViewModel>> { Success = false, Error = ex.Message };
            }
        }

        public async Task<ApiResponseViewModel<UserViewModel>> GetUserByIdAsync(string userId)
        {
            try
            {
                SetAuthHeader();
                var response = await client.GetAsync($"{apiBaseUrl}/{userId}");
                var result = JsonConvert.DeserializeObject<ApiResponseViewModel<UserViewModel>>(await response.Content.ReadAsStringAsync());
                return result ?? new ApiResponseViewModel<UserViewModel> { Success = false, Error = "Invalid response format." };
            }
            catch (Exception ex)
            {
                return new ApiResponseViewModel<UserViewModel> { Success = false, Error = ex.Message };
            }
        }

        public async Task<ApiResponseViewModel<UserViewModel>> RegisterUserAsync(CreateUserViewModel newUser)
        {
            try
            {
                SetAuthHeader();
                var content = new StringContent(JsonConvert.SerializeObject(newUser), Encoding.UTF8, "application/json");
                var response = await client.PostAsync(apiBaseUrl, content);
                var result = JsonConvert.DeserializeObject<ApiResponseViewModel<UserViewModel>>(await response.Content.ReadAsStringAsync());
                return result ?? new ApiResponseViewModel<UserViewModel> { Success = false, Error = "Invalid response format." };
            }
            catch (Exception ex)
            {
                return new ApiResponseViewModel<UserViewModel> { Success = false, Error = ex.Message };
            }
        }

        public async Task<ApiResponseViewModel<UserViewModel>> UpdateUserAsync(UpdateUserViewModel updatedUser)
        {
            try
            {
                SetAuthHeader();
                var content = new StringContent(JsonConvert.SerializeObject(updatedUser), Encoding.UTF8, "application/json");
                var response = await client.PutAsync(apiBaseUrl, content);
                var result = JsonConvert.DeserializeObject<ApiResponseViewModel<UserViewModel>>(await response.Content.ReadAsStringAsync());
                return result ?? new ApiResponseViewModel<UserViewModel> { Success = false, Error = "Invalid response format." };
            }
            catch (Exception ex)
            {
                return new ApiResponseViewModel<UserViewModel> { Success = false, Error = ex.Message };
            }
        }

        public async Task<ApiResponseViewModel<string>> ChangePasswordAsync(ChangePasswordViewModel changePassword)
        {
            try
            {
                SetAuthHeader();
                var content = new StringContent(JsonConvert.SerializeObject(changePassword), Encoding.UTF8, "application/json");
                var response = await client.PostAsync($"{apiBaseUrl}/change-password", content);
                var result = JsonConvert.DeserializeObject<ApiResponseViewModel<string>>(await response.Content.ReadAsStringAsync());
                return result ?? new ApiResponseViewModel<string> { Success = false, Error = "Invalid response format." };
            }
            catch (Exception ex)
            {
                return new ApiResponseViewModel<string> { Success = false, Error = ex.Message };
            }
        }

        public async Task<ApiResponseViewModel<string>> AdminResetPasswordAsync(string email, string temporaryPassword)
        {
            try
            {
                SetAuthHeader();
                var payload = new { email, temporaryPassword };
                var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
                var response = await client.PostAsync($"{apiBaseUrl}/admin-reset-password", content);
                var result = JsonConvert.DeserializeObject<ApiResponseViewModel<string>>(await response.Content.ReadAsStringAsync());
                return result ?? new ApiResponseViewModel<string> { Success = false, Error = "Invalid response format." };
            }
            catch (Exception ex)
            {
                return new ApiResponseViewModel<string> { Success = false, Error = ex.Message };
            }
        }

        public async Task<ApiResponseViewModel<string>> DeleteUserAsync(string userId)
        {
            try
            {
                SetAuthHeader();
                var response = await client.DeleteAsync($"{apiBaseUrl}/{userId}");
                var result = JsonConvert.DeserializeObject<ApiResponseViewModel<string>>(await response.Content.ReadAsStringAsync());
                return result ?? new ApiResponseViewModel<string> { Success = false, Error = "Invalid response format." };
            }
            catch (Exception ex)
            {
                return new ApiResponseViewModel<string> { Success = false, Error = ex.Message };
            }
        }
    }
}
