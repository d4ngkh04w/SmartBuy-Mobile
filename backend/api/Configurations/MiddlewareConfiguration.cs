using api.Helpers;
using api.Middlewares;

namespace api.Configurations
{
    public static class MiddlewareConfiguration
    {
        public static WebApplication ConfigureMiddlewarePipeline(this WebApplication app)
        {
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            else
            {
                app.UseHttpsRedirection();
                app.UseHsts();
            }

            app.UseStaticFiles();
            app.UseRateLimiter();
            app.UseCors("AllowFrontend");
            app.UseAuthentication();
            app.UseAuthorization();

            var httpContextAccessor = app.Services.GetRequiredService<IHttpContextAccessor>();
            CookieHelper.Configure(httpContextAccessor);
            HttpContextHelper.Configure(httpContextAccessor);

            app.UseMiddleware<ErrorHandlerMiddleware>();
            app.UseMiddleware<LoggingMiddleware>();

            app.MapControllers();

            return app;
        }
    }
}
