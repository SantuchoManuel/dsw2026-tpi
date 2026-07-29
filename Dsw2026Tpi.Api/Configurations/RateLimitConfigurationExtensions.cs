using Dsw2026Tpi.CrossCutting.Resources;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.RateLimiting;

namespace Dsw2026Tpi.Api.Configurations
{
    public static class RateLimitConfigurationExtensions
    {
        public static IServiceCollection AddAppRateLimiter(this IServiceCollection services)
        {
            services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

                options.OnRejected = async (context, token) =>
                {

                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("RateLimiting");
                    logger.LogWarning("Petición rechazada por Rate Limiting. Endpoint: {Path}, IP/User: {Connection}",
                        context.HttpContext.Request.Path,
                        context.HttpContext.Connection.RemoteIpAddress);


                    context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    context.HttpContext.Response.ContentType = "application/json";

                    var errorResponse = new
                    {
                        errorCode = nameof(ErrorCodes.TOO_MANY_REQUESTS),
                        message = ErrorCodes.TOO_MANY_REQUESTS,
                        details = new[]
                        {
                          new
                          {
                            field = "request",
                            issue = "Se solicitaron muchas peticiones"
                          }
                        }
                    };
                    var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
                    await context.HttpContext.Response.WriteAsJsonAsync(errorResponse, jsonOptions, token);
                };


                options.AddPolicy("AdminAuthPolicy", httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        factory: partition => new FixedWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit = 5,
                            QueueLimit = 0,
                            Window = TimeSpan.FromMinutes(1)
                        }));


                options.AddPolicy("PatientAuthPolicy", httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        factory: partition => new FixedWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit = 10,
                            QueueLimit = 0,
                            Window = TimeSpan.FromMinutes(1)
                        }));

                options.AddPolicy("TurnosPolicy", httpContext =>
                {

                    var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "unknown_user";

                    return RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: userId,
                        factory: partition => new FixedWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit = 5,
                            QueueLimit = 0,
                            Window = TimeSpan.FromMinutes(1)
                        });
                });

                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                {

                    var isAuth = httpContext.User.Identity?.IsAuthenticated == true;
                    var partitionKey = isAuth
                        ? httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "unknown_user"
                        : httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown_ip";

                    return RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: partitionKey,
                        factory: partition => new FixedWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit = 100,
                            QueueLimit = 0,
                            Window = TimeSpan.FromMinutes(1)
                        });
                });
            });

            return services;
        }
    }
}
