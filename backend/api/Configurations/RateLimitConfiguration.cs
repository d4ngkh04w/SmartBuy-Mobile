using System.Threading.RateLimiting;

namespace api.Configurations
{
    public static class RateLimitConfiguration
    {
        public static IServiceCollection AddRateLimitConfiguration(this IServiceCollection services)
        {
            services.AddRateLimiter(options =>
            {
                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                {
                    var remoteIpAddress = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                    var path = httpContext.Request.Path.ToString().ToLower();

                    if (path.EndsWith("/verify"))
                    {
                        return RateLimitPartition.GetSlidingWindowLimiter(remoteIpAddress, _ => new SlidingWindowRateLimiterOptions
                        {
                            Window = TimeSpan.FromSeconds(10),
                            PermitLimit = 35,
                            SegmentsPerWindow = 5,
                            QueueLimit = 2,
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst
                        });
                    }
                    else if (path.Contains("/auth/"))
                    {
                        return RateLimitPartition.GetSlidingWindowLimiter(remoteIpAddress, _ => new SlidingWindowRateLimiterOptions
                        {
                            Window = TimeSpan.FromSeconds(10),
                            PermitLimit = 10,
                            SegmentsPerWindow = 6,
                            QueueLimit = 2,
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst
                        });
                    }
                    else if (path.Contains("/admin/"))
                    {
                        return RateLimitPartition.GetSlidingWindowLimiter(remoteIpAddress, _ => new SlidingWindowRateLimiterOptions
                        {
                            Window = TimeSpan.FromSeconds(10),
                            PermitLimit = 20,
                            SegmentsPerWindow = 5,
                            QueueLimit = 2,
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst
                        });
                    }
                    else
                    {
                        return RateLimitPartition.GetSlidingWindowLimiter(remoteIpAddress, _ => new SlidingWindowRateLimiterOptions
                        {
                            Window = TimeSpan.FromSeconds(10),
                            PermitLimit = 30,
                            SegmentsPerWindow = 5,
                            QueueLimit = 1,
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst
                        });
                    }
                });

                options.OnRejected = async (context, token) =>
                {
                    context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    context.HttpContext.Response.ContentType = "application/json";
                    await context.HttpContext.Response.WriteAsJsonAsync(new
                    {
                        Message = "Too many requests. Please try again later"
                    }, cancellationToken: token);
                };
            });

            return services;
        }
    }
}
