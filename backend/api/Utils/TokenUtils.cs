using System.Security.Cryptography;
using System.Text;

namespace api.Utils
{
    public static class TokenUtils
    {
        public static string GenerateToken(int byteLength = 64)
        {
            var randomBytes = new byte[byteLength];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return CleanBase64(Convert.ToBase64String(randomBytes));
        }

        private static string CleanBase64(string base64String)
        {
            return base64String
                .Replace("/", "_")
                .Replace("+", "-")
                .Replace("=", "");
        }

        public static string HashToken(string token)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
            return Convert.ToHexString(bytes).ToLower();
        }
    }
}