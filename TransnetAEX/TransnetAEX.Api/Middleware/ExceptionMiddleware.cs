using System.Net;
using System.Text.Json;
using TransnetAEX.Api.DTOs.Shared;

namespace TransnetAEX.Api.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
        private readonly IWebHostEnvironment _env;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IWebHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Exception caught in middleware: {ex.Message}");
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            // Default to 500 unless we detect specific exceptions
            var statusCode = (int)HttpStatusCode.InternalServerError;

            // Customize HTTP status codes for specific cases
            if (exception is UnauthorizedAccessException)
                statusCode = (int)HttpStatusCode.Unauthorized;
            else if (exception is KeyNotFoundException)
                statusCode = (int)HttpStatusCode.NotFound;
            else if (exception is ArgumentException)
                statusCode = (int)HttpStatusCode.BadRequest;

            context.Response.StatusCode = statusCode;

            // Return standardized JSON response
            var response = ApiResponseDTO<object>.FailResponse(
                exception.Message,
                _env.IsDevelopment()
                    ? $"An unexpected error occurred: {exception.Message}\n\nStack Trace:\n{exception.StackTrace}"
                    : "An unexpected server error occurred. Please contact the administrator."
            );

            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
        }
    }
}
