using Microsoft.AspNetCore.RateLimiting;
using static Microsoft.AspNetCore.RateLimiting.RateLimiterOptionsExtensions;
namespace BlockedCountriesApi.Configuration;

public static class RateLimitingConfig
{
    public static void ConfigureRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            // Global rate limiter for all endpoints
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: context.User.Identity?.Name ?? context.Request.Headers.Host.ToString(),
                    factory: partition => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = 100,
                        Window = TimeSpan.FromMinutes(1)
                    }));

            // IP lookup endpoint - more restrictive
            options.AddPolicy("ip-lookup", httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    factory: partition => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = 30,
                        Window = TimeSpan.FromMinutes(1)
                    }));

            // Block check endpoint - most restrictive
            options.AddPolicy("block-check", httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    factory: partition => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = 10,
                        Window = TimeSpan.FromMinutes(1)
                    }));

            // Admin endpoints (block/unblock) - less restrictive
            options.AddPolicy("admin", httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: httpContext.User.Identity?.Name ?? httpContext.Request.Headers.Host.ToString(),
                    factory: partition => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = 50,
                        Window = TimeSpan.FromMinutes(1)
                    }));

            // Configure rate limit exceeded response
            options.OnRejected = async (context, token) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                await context.HttpContext.Response.WriteAsJsonAsync(new
                {
                    Error = "Too many requests. Please try again later.",
                    RetryAfter = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter) 
                        ? retryAfter.TotalSeconds 
                        : null
                }, token);
            };
        });
    }
} 