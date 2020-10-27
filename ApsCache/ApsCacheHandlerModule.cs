// ASP.NET Core middleware

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace ApsCache
{
    public class ApsCacheModule
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;

        public ApsCacheModule(ILogger<ApsCacheModule> logger,RequestDelegate next)
        {
            _logger=logger;
            _next = next;
        }

        public bool TerminateRequest(){
            return false;
        }

        public async Task Invoke(HttpContext context)
        {
            // Do something with context near the beginning of request processing.
            _logger.LogInformation("Entering ApsCacheHandler.Invoke");

            if (!TerminateRequest()){
                await _next.Invoke(context);
            }
            // Clean up.
        }
    }

    public static class HandlerExtensions
    {
        public static IApplicationBuilder UseApsCacheModule(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ApsCacheModule>();
        }
    }
}