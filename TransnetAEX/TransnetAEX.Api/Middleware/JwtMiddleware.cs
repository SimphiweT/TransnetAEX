using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TransnetAEX.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace TransnetAEX.Api.Middleware
{
    public class JwtMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _config;

        public JwtMiddleware(RequestDelegate next, IConfiguration config)
        {
            _next = next;
            _config = config;
        }

        public async Task Invoke(HttpContext context, UserManager<SystemUser> userManager)
        {
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            if (!string.IsNullOrEmpty(token))
                await AttachUserToContext(context, userManager, token);

            await _next(context);
        }

        private async Task AttachUserToContext(HttpContext context, UserManager<SystemUser> userManager, string token)
        {
            try
            {
                var key = Encoding.UTF8.GetBytes(_config["Jwt:Key"] ?? string.Empty);
                var tokenHandler = new JwtSecurityTokenHandler();
                var parameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = _config["Jwt:Issuer"],
                    ValidAudience = _config["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ClockSkew = TimeSpan.Zero
                };

                var principal = tokenHandler.ValidateToken(token, parameters, out var validatedToken);
                var userId = principal.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;

                if (userId != null)
                {
                    var user = await userManager.FindByIdAsync(userId);
                    if (user != null)
                    {
                        context.Items["User"] = user;
                        context.Items["UserEmail"] = user.Email;
                        context.Items["UserDisplayName"] = user.UserName;
                        context.Items["UserRoles"] = principal.Claims
                            .Where(c => c.Type == ClaimTypes.Role)
                            .Select(c => c.Value)
                            .ToList();
                    }
                }
            }
            catch
            {
                // Ignore JWT validation errors silently — user will simply not be attached.
            }
        }
    }
}
