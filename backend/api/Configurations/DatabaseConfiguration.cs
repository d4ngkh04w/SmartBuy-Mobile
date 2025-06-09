using api.Database;
using api.Helpers;
using Microsoft.EntityFrameworkCore;

namespace api.Configurations
{
    public static class DatabaseConfiguration
    {
        public static IServiceCollection AddDatabaseConfiguration(this IServiceCollection services)
        {
            services.AddDbContext<AppDBContext>(options =>
                options.UseMySql(ConfigHelper.ConnectionString,
                new MySqlServerVersion(new Version(9, 2, 0))));

            return services;
        }
    }
}
