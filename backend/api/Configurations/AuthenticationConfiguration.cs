using System.Text;
using api.Helpers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace api.Configurations
{
    public static class AuthenticationConfiguration
    {
        public static IServiceCollection AddAuthenticationConfiguration(this IServiceCollection services)
        {
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "smart";
                options.DefaultChallengeScheme = "smart";
                options.DefaultScheme = "smart";
                options.DefaultForbidScheme = "smart";
                options.DefaultSignInScheme = "smart";
                options.DefaultSignOutScheme = "smart";
            })
            .AddPolicyScheme("smart", "Smart Auth Selector", options =>
            {
                options.ForwardDefaultSelector = context =>
                {
                    string origin = HttpContextHelper.UserOrigin;

                    if (string.IsNullOrWhiteSpace(origin) || origin.Contains("unknown"))
                    {
                        return "both";
                    }
                    // For localhost development
                    else if (origin.Contains("localhost:5000"))
                    {
                        return "both";
                    }
                    else if (origin.Contains(ConfigHelper.AdminUrl))
                    {
                        return "admin";
                    }

                    return "user";
                };
            })
            .AddJwtBearer("admin", ConfigureAdminJwtBearer)
            .AddJwtBearer("user", ConfigureUserJwtBearer)
            .AddJwtBearer("both", ConfigureBothJwtBearer);

            return services;
        }

        private static void ConfigureAdminJwtBearer(JwtBearerOptions options)
        {
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var token = CookieHelper.AdminAccessToken;
                    if (!string.IsNullOrEmpty(token))
                    {
                        context.Token = token;
                    }
                    return Task.CompletedTask;
                },
            };
            options.TokenValidationParameters = GetTokenValidationParameters();
        }

        private static void ConfigureUserJwtBearer(JwtBearerOptions options)
        {
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var token = CookieHelper.UserAccessToken;
                    if (!string.IsNullOrEmpty(token))
                    {
                        context.Token = token;
                    }
                    return Task.CompletedTask;
                },
            };
            options.TokenValidationParameters = GetTokenValidationParameters();
        }

        private static void ConfigureBothJwtBearer(JwtBearerOptions options)
        {
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var adminToken = CookieHelper.AdminAccessToken;
                    if (!string.IsNullOrEmpty(adminToken))
                    {
                        context.Token = adminToken;
                        return Task.CompletedTask;
                    }

                    var userToken = CookieHelper.UserAccessToken;
                    if (!string.IsNullOrEmpty(userToken))
                    {
                        context.Token = userToken;
                    }
                    return Task.CompletedTask;
                },
                OnAuthenticationFailed = context =>
                {
                    if (context.Exception != null && !string.IsNullOrEmpty(CookieHelper.AdminAccessToken) && !string.IsNullOrEmpty(CookieHelper.UserAccessToken))
                    {
                        Console.WriteLine($"Authentication failed: {context.Exception.Message}");
                    }
                    return Task.CompletedTask;
                }
            };
            options.TokenValidationParameters = GetTokenValidationParameters();
        }

        private static TokenValidationParameters GetTokenValidationParameters()
        {
            return new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,

                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(ConfigHelper.JwtSecretKey)),
                ValidIssuer = ConfigHelper.JwtIssuer,
                ValidAudience = ConfigHelper.JwtAudience,
            };
        }
    }
}
