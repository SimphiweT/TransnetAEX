using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using TransnetAEX.Web.Api_Services.IServices;
using TransnetAEX.Web.Helpers;
using TransnetAEX.Web.Models.Auth;
using TransnetAEX.Web.Models.Shared;

namespace TransnetAEX.Web.Api_Services.Services
{
    public class AccessService : IAccessService
    {
        private readonly HttpClient client;
        private readonly ApiSettings apiSettings;
        private readonly SessionManager sessionManager;
        private readonly string apiBaseUrl;

        public AccessService(HttpClient _client, IOptions<ApiSettings> _apiSettings, SessionManager _sessionManager)
        {
            client = _client;
            apiSettings = _apiSettings.Value;
            sessionManager = _sessionManager;
            apiBaseUrl = apiSettings.BaseUrl + "access";
        }

        private void SetAuthHeader()
        {
            var token = sessionManager.GetToken();
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        public async Task<ApiResponseViewModel<List<AccessResponseViewModel>>> GetAllAccessRequestsAsync()
        {
            try
            {
                SetAuthHeader();
                var response = await client.GetAsync($"{apiBaseUrl}/requests");
                var result = JsonConvert.DeserializeObject<ApiResponseViewModel<List<AccessResponseViewModel>>>(await response.Content.ReadAsStringAsync());
                return result ?? new ApiResponseViewModel<List<AccessResponseViewModel>> { Success = false, Error = "Invalid response format." };
            }
            catch (Exception ex)
            {
                return new ApiResponseViewModel<List<AccessResponseViewModel>> { Success = false, Error = ex.Message };
            }
        }

        public async Task<ApiResponseViewModel<string>> ApproveAccessRequestAsync(int id)
        {
            try
            {
                SetAuthHeader();
                var response = await client.PostAsync($"{apiBaseUrl}/approve/{id}", null);
                var result = JsonConvert.DeserializeObject<ApiResponseViewModel<string>>(await response.Content.ReadAsStringAsync());
                return result ?? new ApiResponseViewModel<string> { Success = false, Error = "Invalid response format." };
            }
            catch (Exception ex)
            {
                return new ApiResponseViewModel<string> { Success = false, Error = ex.Message };
            }
        }

        public async Task<ApiResponseViewModel<string>> DeclineAccessRequestAsync(int id, string reason)
        {
            try
            {
                SetAuthHeader();
                var content = new StringContent(JsonConvert.SerializeObject(new { reason }), System.Text.Encoding.UTF8, "application/json");
                var response = await client.PostAsync($"{apiBaseUrl}/decline/{id}", content);
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
