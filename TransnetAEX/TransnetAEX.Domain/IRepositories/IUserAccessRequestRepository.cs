using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransnetAEX.Domain.Entities;
using TransnetAEX.Domain.IRepositories.Base;

namespace TransnetAEX.Domain.IRepositories
{
    /// <summary>
    /// Repository contract for managing user access requests.
    /// </summary>
    public interface IUserAccessRequestRepository : IRepositoryBase<UserAccessRequest>
    {
        /// <summary>Retrieves all pending access requests (not approved or declined).</summary>
        Task<IEnumerable<UserAccessRequest>> GetPendingRequestsAsync();

        /// <summary>Retrieves all reviewed access requests (approved or declined).</summary>
        Task<IEnumerable<UserAccessRequest>> GetReviewedRequestsAsync();
    }
}
