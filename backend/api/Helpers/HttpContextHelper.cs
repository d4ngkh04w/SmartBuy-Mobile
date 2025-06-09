using System.Security.Claims;

namespace api.Helpers
{
    public static class HttpContextHelper
    {
        private static IHttpContextAccessor? _httpContextAccessor;
        public static HttpContext? Current => _httpContextAccessor?.HttpContext;

        public static void Configure(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public static Guid CurrentUserId => Guid.TryParse(Current?.User.Claims.FirstOrDefault(c => c.Type == "id")?.Value, out var userId) ? userId : Guid.Empty;
        public static string CurrentUserRole => Current?.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value ?? string.Empty;
        public static string CurrentUserEmail => Current?.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value ?? string.Empty;
        public static string UserIP
        {
            get
            {
                var ip = Current?.Connection.RemoteIpAddress?.ToString();
                return string.IsNullOrEmpty(ip) ? "unknown" : ip switch
                {
                    "::1" => "localhost",
                    _ => ip
                };
            }
        }
        public static string UserAgent => Current?.Request.Headers.UserAgent.ToString() ?? "unknown";
        public static string UserOrigin
        {
            get
            {
                var origin = Current?.Request.Headers.Origin.ToString();
                return string.IsNullOrEmpty(origin) ? "unknown" : origin;
            }
        }
    }
}