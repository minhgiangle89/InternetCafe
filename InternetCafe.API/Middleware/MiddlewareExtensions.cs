using InternetCafe.API.Middleware;
using Microsoft.AspNetCore.Builder;

namespace InternetCafe.API.Extensions
{
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseErrorHandling(this IApplicationBuilder app)
        {
            return app.UseMiddleware<ErrorHandlingMiddleware>();
        }
    }
}