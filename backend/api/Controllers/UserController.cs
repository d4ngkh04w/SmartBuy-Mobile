using api.DTOs.User;
using api.Helpers;
using api.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [Authorize(AuthenticationSchemes = "smart", Roles = "admin,user")]
    public class UserController : BaseController
    {
        private readonly IUserService _userService;
        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        [Authorize(AuthenticationSchemes = "admin", Roles = "admin")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return ApiResponseHelper.Success("Users retrieved successfully", users);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetUserById([FromRoute] Guid id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            return ApiResponseHelper.Success("User retrieved successfully", user);
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            var id = HttpContextHelper.CurrentUserId;
            var user = await _userService.GetUserByIdAsync(id);
            return ApiResponseHelper.Success("User retrieved successfully", user);
        }

        [HttpPut("me")]
        [RequestSizeLimit(15 * 1024 * 1024)]
        public async Task<IActionResult> UpdateCurrentUser([FromForm] UpdateUserDTO userDTO)
        {
            var id = HttpContextHelper.CurrentUserId;
            var user = await _userService.UpdateUserAsync(id, userDTO);
            return ApiResponseHelper.Success("User updated successfully", user);
        }

        [HttpDelete("me")]
        [Authorize(AuthenticationSchemes = "user", Roles = "user")]
        public async Task<IActionResult> DeleteCurrentUser([FromBody] DeleteAccountDTO deleteAccountDTO)
        {
            var id = HttpContextHelper.CurrentUserId;
            await _userService.DeleteUserAsync(id, deleteAccountDTO.Password);
            CookieHelper.RemoveAuthUserTokens();
            return NoContent();
        }

        [HttpPut("{id:guid}/lock")]
        [Authorize(AuthenticationSchemes = "admin", Roles = "admin")]
        public async Task<IActionResult> LockUser([FromRoute] Guid id, [FromBody] LockUserDTO lockUserDTO)
        {
            await _userService.LockUserAsync(id, lockUserDTO, "admin");
            return ApiResponseHelper.Success<object>("User locked successfully", null);
        }

        [HttpPut("{id:guid}/unlock")]
        [Authorize(AuthenticationSchemes = "admin", Roles = "admin")]
        public async Task<IActionResult> UnlockUser([FromRoute] Guid id)
        {
            await _userService.UnlockUserAsync(id);
            return ApiResponseHelper.Success<object>("User unlocked successfully", null);
        }

        [HttpPut("me/lock")]
        [Authorize(AuthenticationSchemes = "user", Roles = "user")]
        public async Task<IActionResult> LockCurrentUser([FromBody] LockUserDTO lockUserDTO)
        {
            var id = HttpContextHelper.CurrentUserId;
            await _userService.LockUserAsync(id, lockUserDTO, "user");

            CookieHelper.RemoveAuthUserTokens();
            return ApiResponseHelper.Success<object>("User locked successfully", null);
        }
    }
}