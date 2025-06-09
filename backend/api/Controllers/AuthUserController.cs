using api.DTOs.Auth;
using api.Helpers;
using api.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [Authorize(AuthenticationSchemes = "user", Roles = "user")]
    public class AuthUserController : BaseUserAuthController
    {
        private readonly IAuthService _authService;

        public AuthUserController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterDTO register)
        {
            var token = await _authService.Register(register, "user");
            CookieHelper.UserRefreshToken = token.RefreshToken;
            CookieHelper.UserAccessToken = token.AccessToken;

            return ApiResponseHelper.Success<object>("User registered successfully", null);
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDTO login)
        {
            var token = await _authService.Login(login, "user");
            CookieHelper.UserAccessToken = token.AccessToken;
            CookieHelper.UserRefreshToken = token.RefreshToken;

            return ApiResponseHelper.Success<object>("Login successful", null);
        }

        [HttpPost("google-login")]
        [AllowAnonymous]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginDTO dto)
        {
            var token = await _authService.LoginWithGoogleAsync(dto, "user");
            CookieHelper.UserAccessToken = token.AccessToken;
            CookieHelper.UserRefreshToken = token.RefreshToken;

            return ApiResponseHelper.Success<object>("Login successful", null);
        }

        [HttpGet("verify")]
        public IActionResult VerifyToken()
        {
            return Ok(new
            {
                Success = true,
                Message = "Token is valid",
                UserId = HttpContextHelper.CurrentUserId,
                Email = HttpContextHelper.CurrentUserEmail,
                Role = HttpContextHelper.CurrentUserRole
            });
        }

        [HttpPost("send-verification-email")]
        public async Task<IActionResult> SendVerificationEmail()
        {
            await _authService.SendEmailVerificationAsync();
            return ApiResponseHelper.Success<object>("Email sent successfully", null);
        }

        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailDTO verifyDto)
        {
            await _authService.VerifyEmailAsync(verifyDto);

            return ApiResponseHelper.Success<object>("Email verified successfully", null);
        }
    }
}