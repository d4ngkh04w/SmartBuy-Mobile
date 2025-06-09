using api.Interfaces.Repositories;
using api.Interfaces.Services;
using api.Repositories;
using api.Services;

namespace api.Configurations
{
    public static class ServiceConfiguration
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IAuthService, AuthService>();

            services.AddScoped<IBrandRepository, BrandRepository>();
            services.AddScoped<IBrandService, BrandService>();

            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IUserTokenRepository, UserTokenRepository>();

            services.AddScoped<IEmailService, EmailService>();

            services.AddScoped<IProductLineRepository, ProductLineRepository>();
            services.AddScoped<IProductLineService, ProductLineService>();

            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<IProductService, ProductService>();

            services.AddScoped<ICartRepository, CartRepository>();
            services.AddScoped<ICartService, CartService>();

            services.AddScoped<ICommentRepository, CommentRepository>();
            services.AddScoped<ICommentService, CommentService>();

            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IOrderService, OrderService>();

            services.AddScoped<IDashboardRepository, DashboardRepository>();
            services.AddScoped<IDashboardService, DashboardService>();

            services.AddScoped<IDiscountRepository, DiscountRepository>();
            services.AddScoped<IDiscountService, DiscountService>();

            services.AddScoped<ICacheService, CacheService>();            services.AddScoped<IChatbotService, ChatbotService>();
            services.AddScoped<GeminiChatbotService>();
            services.AddHttpClient();

            return services;
        }

        public static IServiceCollection AddMemoryCacheConfiguration(this IServiceCollection services)
        {
            services.AddMemoryCache(options =>
            {
                options.ExpirationScanFrequency = TimeSpan.FromMinutes(5);
                options.SizeLimit = 15000;
            });

            return services;
        }
    }
}