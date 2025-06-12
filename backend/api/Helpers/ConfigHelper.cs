namespace api.Helpers
{
    public static class ConfigHelper
    {
        private static string? _googleClientId;
        private static string? _connectionString;
        private static string? _jwtSecretKey;
        private static string? _jwtIssuer;
        private static string? _jwtAudience;
        private static string? _jwtAlgorithm;
        private static double? _jwtExpireTime;
        private static double? _jwtRefreshTokenExpiry;
        private static string? _emailDisplayName;
        private static string? _emailSender;
        private static string? _emailHost;
        private static int? _emailPort;
        private static string? _email;
        private static string? _emailPassword;
        private static string? _adminPassword;
        private static string? _adminPhoneNumber;
        private static string? _adminName;
        private static string? _adminUrl;
        private static string? _userUrl;
        private static string? _geminiApiKey;

        public static void Initialize(IConfiguration configuration)
        {
            _googleClientId = configuration["Google:ClientId"] ?? throw new InvalidOperationException("Google Client ID not found in configuration");
            _connectionString = configuration["ConnectionStrings:DefaultConnection"] ?? throw new InvalidOperationException("Database connection string not found in configuration");
            _jwtSecretKey = configuration["JWT:Key"] ?? throw new InvalidOperationException("JWT Secret Key not found in configuration");
            _jwtIssuer = configuration["JWT:Issuer"] ?? throw new InvalidOperationException("JWT Issuer not found in configuration");
            _jwtAudience = configuration["JWT:Audience"] ?? throw new InvalidOperationException("JWT Audience not found in configuration");
            _jwtAlgorithm = configuration["JWT:Algorithm"] ?? "HS256";
            _jwtExpireTime = double.TryParse(configuration["JWT:Expire"], out var expire) ? expire : 30;
            _jwtRefreshTokenExpiry = double.TryParse(configuration["JWT:RefreshTokenExpiry"], out var refreshTokenExpiry) ? refreshTokenExpiry : 7;
            _emailDisplayName = configuration["EmailSettings:DisplayName"] ?? "SmartBuy Mobile";
            _emailSender = configuration["EmailSettings:From"] ?? throw new InvalidOperationException("Email Sender not found in configuration");
            _emailHost = configuration["EmailSettings:Host"] ?? "smtp.gmail.com";
            _emailPort = int.TryParse(configuration["EmailSettings:Port"], out var port) ? port : 587;
            _email = configuration["EmailSettings:Email"] ?? throw new InvalidOperationException("Email not found in configuration");
            _emailPassword = configuration["EmailSettings:Password"] ?? throw new InvalidOperationException("Email Password not found in configuration");
            _adminPassword = configuration["Admin:Password"] ?? throw new InvalidOperationException("Admin Password not found in configuration");
            _adminPhoneNumber = configuration["Admin:PhoneNumber"] ?? "0333666999";
            _adminName = configuration["Admin:Name"] ?? "SmartBuy Mobile";
            _adminUrl = configuration["Frontend:AdminUrl"] ?? "http://localhost:4000";
            _userUrl = configuration["Frontend:UserUrl"] ?? "http://localhost:3000";
            _geminiApiKey = configuration["Gemini:ApiKey"] ?? string.Empty;
        }

        public static string GoogleClientId => _googleClientId!;
        public static string ConnectionString => _connectionString!;
        public static string JwtSecretKey => _jwtSecretKey!;
        public static string JwtIssuer => _jwtIssuer!;
        public static string JwtAudience => _jwtAudience!;
        public static string JwtAlgorithm => _jwtAlgorithm!;
        public static double JwtExpireTime => _jwtExpireTime!.Value;
        public static double JwtRefreshTokenExpiry => _jwtRefreshTokenExpiry!.Value;
        public static string EmailDisplayName => _emailDisplayName!;
        public static string EmailSender => _emailSender!;
        public static string EmailHost => _emailHost!;
        public static int EmailPort => _emailPort!.Value;
        public static string Email => _email!;
        public static string EmailPassword => _emailPassword!;
        public static string AdminPassword => _adminPassword!;
        public static string AdminPhoneNumber => _adminPhoneNumber!;
        public static string AdminName => _adminName!;
        public static string AdminUrl => _adminUrl!;
        public static string UserUrl => _userUrl!;
        public static string GeminiApiKey => _geminiApiKey!;
    }
}