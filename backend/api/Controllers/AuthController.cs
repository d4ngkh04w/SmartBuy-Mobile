using api.DTOs.Auth;
using api.Helpers;
using api.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [Authorize(AuthenticationSchemes = "smart", Roles = "admin,user")]
    public class AuthController : BaseController
    {
        private readonly IAuthService _authService;
        private readonly ITokenService _tokenService;

        public AuthController(IAuthService authService, ITokenService tokenService)
        {
            _authService = authService;
            _tokenService = tokenService;
        }

        [HttpPost("refresh-token")]
        [AllowAnonymous]
        public async Task<IActionResult> RefreshToken()
        {
            if (HttpContextHelper.UserOrigin.Contains(ConfigHelper.AdminUrl))
            {
                var adminRefreshToken = CookieHelper.AdminRefreshToken;
                var token = await _tokenService.ValidateRefreshToken(adminRefreshToken, "admin");
                CookieHelper.AdminAccessToken = token.AccessToken;
                CookieHelper.AdminRefreshToken = token.RefreshToken;
            }
            else if (HttpContextHelper.UserOrigin.Contains(ConfigHelper.UserUrl))
            {
                var userRefreshToken = CookieHelper.UserRefreshToken;
                var token = await _tokenService.ValidateRefreshToken(userRefreshToken, "user");
                CookieHelper.UserAccessToken = token.AccessToken;
                CookieHelper.UserRefreshToken = token.RefreshToken;
            }
            else if (HttpContextHelper.UserOrigin == "unknown" || string.IsNullOrEmpty(HttpContextHelper.UserOrigin))
            {
                var adminRefreshToken = CookieHelper.AdminRefreshToken;
                if (!string.IsNullOrEmpty(adminRefreshToken))
                {
                    var adminToken = await _tokenService.ValidateRefreshToken(adminRefreshToken, "admin");
                    CookieHelper.AdminAccessToken = adminToken.AccessToken;
                    CookieHelper.AdminRefreshToken = adminToken.RefreshToken;
                }

                var userRefreshToken = CookieHelper.UserRefreshToken;
                if (!string.IsNullOrEmpty(userRefreshToken))
                {
                    var userToken = await _tokenService.ValidateRefreshToken(userRefreshToken, "user");
                    CookieHelper.UserAccessToken = userToken.AccessToken;
                    CookieHelper.UserRefreshToken = userToken.RefreshToken;
                }
            }

            return ApiResponseHelper.Success<object>("Token refreshed successfully", null);
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            if (HttpContextHelper.UserOrigin.Contains(ConfigHelper.AdminUrl))
            {
                var adminRefreshToken = CookieHelper.AdminRefreshToken;
                if (!string.IsNullOrEmpty(adminRefreshToken))
                {
                    await _tokenService.RevokeRefreshToken(adminRefreshToken);
                    CookieHelper.RemoveAuthAdminTokens();
                }
            }
            else if (HttpContextHelper.UserOrigin.Contains(ConfigHelper.UserUrl))
            {
                var userRefreshToken = CookieHelper.UserRefreshToken;
                if (!string.IsNullOrEmpty(userRefreshToken))
                {
                    await _tokenService.RevokeRefreshToken(userRefreshToken);
                    CookieHelper.RemoveAuthUserTokens();
                }
            }
            else
            {
                CookieHelper.RemoveAuthAdminTokens();
                CookieHelper.RemoveAuthUserTokens();
            }
            return ApiResponseHelper.Success<object>("Logged out successfully", null);
        }

        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDTO forgotPasswordDto)
        {
            await _authService.ForgotPasswordAsync(forgotPasswordDto);
            return ApiResponseHelper.Success<object>("If the email address exists in our system, we will send a password reset link", null);
        }

        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDTO resetPasswordDto)
        {
            await _authService.ResetPasswordAsync(resetPasswordDto);
            return ApiResponseHelper.Success<object>("Password has been reset successfully. You can now log in with your new password", null);
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDTO changePasswordDto)
        {
            var userId = HttpContextHelper.CurrentUserId;
            if (userId == Guid.Empty)
                return Unauthorized(new { Message = "User not authenticated" });

            await _authService.ChangePasswordAsync(changePasswordDto, userId);

            return ApiResponseHelper.Success<object>("Password changed successfully", null);
        }
    }
}