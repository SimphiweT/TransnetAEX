using TransnetAEX.Web.Models.Auth;
using TransnetAEX.Web.Models.Shared;

namespace TransnetAEX.Web.Api_Services.IServices
{
    public interface IAccessService
    {
        Task<ApiResponseViewModel<List<AccessResponseViewModel>>> GetAllAccessRequestsAsync();
        Task<ApiResponseViewModel<string>> ApproveAccessRequestAsync(int id);
        Task<ApiResponseViewModel<string>> DeclineAccessRequestAsync(int id, string reason);
    }
}
