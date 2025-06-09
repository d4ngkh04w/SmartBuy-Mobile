using api.DTOs.Cart;
using api.Exceptions;
using api.Helpers;
using api.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [Authorize(AuthenticationSchemes = "user", Roles = "user")]
    public class CartController : BaseController
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        [HttpGet]
        public async Task<IActionResult> GetCart()
        {
            var userId = HttpContextHelper.CurrentUserId;

            var cart = await _cartService.GetCartAsync(userId);
            return ApiResponseHelper.Success("Cart retrieved successfully", cart);
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartDTO dto)
        {
            var userId = HttpContextHelper.CurrentUserId;
            if (userId == Guid.Empty)
                throw new UnauthorizedException();

            var cart = await _cartService.AddToCartAsync(userId, dto);
            return ApiResponseHelper.Success("Product added to cart successfully", cart);
        }

        [HttpPut("items/{itemId:guid}")]
        public async Task<IActionResult> UpdateCartItem([FromRoute] Guid itemId, [FromBody] UpdateCartItemDTO dto)
        {
            var userId = HttpContextHelper.CurrentUserId;

            var cart = await _cartService.UpdateCartItemAsync(userId, itemId, dto);
            return ApiResponseHelper.Success("Cart item updated successfully", cart);
        }

        [HttpDelete("items/{itemId:guid}")]
        public async Task<IActionResult> RemoveCartItem([FromRoute] Guid itemId)
        {
            var userId = HttpContextHelper.CurrentUserId;
            if (userId == Guid.Empty)
                throw new UnauthorizedException();

            await _cartService.RemoveCartItemAsync(userId, itemId);
            return NoContent();
        }

        [HttpDelete("clear")]
        public async Task<IActionResult> ClearCart()
        {
            var userId = HttpContextHelper.CurrentUserId;
            if (userId == Guid.Empty)
                throw new UnauthorizedException();

            await _cartService.ClearCartAsync(userId);

            return NoContent();
        }
    }
}