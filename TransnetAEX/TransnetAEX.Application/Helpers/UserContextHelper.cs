using Microsoft.AspNetCore.Http;
using System.Net.Http;
using System.Security.Claims;
using TransnetAEX.Domain.Entities;

namespace TransnetAEX.Application.Helpers
{
    public static class UserContextHelper
    {
        public static string? GetUserEmail(HttpContext context)
        {
            return context.Items["UserEmail"] as string;
        }

        public static string? GetUserDisplayName(HttpContext context)
        {
            return context.Items["UserDisplayName"] as string;
        }

        public static List<string> GetUserRoles(HttpContext context)
        {
            return context.Items["UserRoles"] as List<string> ?? new();
        }

        public static SystemUser? GetUser(HttpContext context)
        {
            return context.Items["User"] as SystemUser;
        }
    }
}
