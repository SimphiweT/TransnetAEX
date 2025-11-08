using TransnetAEX.Api.Middleware;

namespace TransnetAEX.Api.Extensions
{
    public static class ExceptionMiddlewareExtensions
    {
        public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
        {
            app.UseMiddleware<ExceptionMiddleware>();
            return app;
        }
    }
}
