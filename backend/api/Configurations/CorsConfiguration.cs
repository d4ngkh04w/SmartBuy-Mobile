using api.Helpers;

namespace api.Configurations
{
    public static class CorsConfiguration
    {
        public static IServiceCollection AddCorsConfiguration(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend",
                    policy => policy.WithOrigins(ConfigHelper.AdminUrl, ConfigHelper.UserUrl)
                                    .WithMethods("GET", "POST", "PUT", "DELETE", "OPTIONS")
                                    .AllowAnyHeader()
                                    .AllowCredentials());
            });

            return services;
        }
    }
}
