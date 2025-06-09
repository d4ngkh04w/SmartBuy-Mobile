using api.DTOs.Auth;
using api.Helpers;
using api.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [Authorize(AuthenticationSchemes = "admin", Roles = "admin")]
    public class AuthAdminController : BaseAdminAuthController
    {
        private readonly IAuthService _authService;
        public AuthAdminController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDTO login)
        {
            var token = await _authService.Login(login, "admin");
            CookieHelper.AdminAccessToken = token.AccessToken;
            CookieHelper.AdminRefreshToken = token.RefreshToken;
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

    }
}