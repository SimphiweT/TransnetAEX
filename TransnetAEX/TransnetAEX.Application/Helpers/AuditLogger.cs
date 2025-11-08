using Microsoft.AspNetCore.Http;
using TransnetAEX.Domain.Entities;
using TransnetAEX.Domain.IRepositories.Base;

namespace TransnetAEX.Application.Helpers
{
    public static class AuditLogger
    {
        public static async Task LogAsync(
    IRepositoryWrapper repo,
    string? userId,
    string displayName,
    string role,
    string actionType,
    HttpContext httpContext,
    string? notes = null,
    bool success = true,
    string? failureReason = null)
        {
            var log = new SessionAuditLog
            {
                UserId = userId ?? "N/A",
                DisplayName = displayName,
                UserRole = role,
                ActionType = actionType,
                IpAddress = httpContext.Connection.RemoteIpAddress?.ToString(),
                DeviceInfo = httpContext.Request.Headers["User-Agent"].ToString(),
                Timestamp = DateTime.UtcNow,
                Notes = notes,
                Success = success,
                FailureReason = failureReason
            };

            await repo.SessionAuditLog.CreateAsync(log);
            await repo.SaveAsync();
        }
    }

}

