using Microsoft.AspNetCore.Mvc;

namespace api.Configurations
{
    public static class ApiConfiguration
    {
        public static IServiceCollection AddApiConfiguration(this IServiceCollection services)
        {
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var errors = context.ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                    return new BadRequestObjectResult(new { Success = false, Message = "Invalid data", Error = errors });
                };
            });

            return services;
        }
    }
}
