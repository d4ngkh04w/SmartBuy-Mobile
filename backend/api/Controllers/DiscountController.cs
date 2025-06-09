using api.DTOs.Discount;
using api.Helpers;
using api.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [Authorize(AuthenticationSchemes = "admin", Roles = "admin")]
    public class DiscountController : BaseController
    {
        private readonly IDiscountService _discountService;

        public DiscountController(IDiscountService discountService)
        {
            _discountService = discountService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetDiscounts()
        {
            var discounts = await _discountService.GetAllDiscountsAsync();
            return ApiResponseHelper.Success("Discounts retrieved successfully", discounts);
        }

        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetDiscountById([FromRoute] int id)
        {
            var discount = await _discountService.GetDiscountByIdAsync(id);
            return ApiResponseHelper.Success("Discount retrieved successfully", discount);
        }

        [HttpPost]
        public async Task<IActionResult> CreateDiscount([FromBody] CreateDiscountDTO discountDTO)
        {
            var discount = await _discountService.CreateDiscountAsync(discountDTO);
            return ApiResponseHelper.Created("Discount created successfully", discount);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateDiscount([FromRoute] int id, [FromBody] UpdateDiscountDTO discountDTO)
        {
            var discount = await _discountService.UpdateDiscountAsync(id, discountDTO);
            return ApiResponseHelper.Success("Discount updated successfully", discount);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteDiscount([FromRoute] int id)
        {
            await _discountService.DeleteDiscountAsync(id);
            return ApiResponseHelper.Success<object>("Discount deleted successfully");
        }

        [HttpGet("{id:int}/products")]
        [AllowAnonymous]
        public async Task<IActionResult> GetProductsByDiscountId([FromRoute] int id)
        {
            var products = await _discountService.GetProductsByDiscountIdAsync(id);
            return ApiResponseHelper.Success("Products retrieved successfully", products);
        }

        [HttpGet("product/{productId:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetDiscountsByProductId([FromRoute] int productId)
        {
            var discounts = await _discountService.GetDiscountsByProductIdAsync(productId);
            return ApiResponseHelper.Success("Discounts retrieved successfully", discounts);
        }

        [HttpPost("{id:int}/products")]
        public async Task<IActionResult> AddProductsToDiscount(
            [FromRoute] int id,
            [FromBody] AddProductToDiscountDTO productIds)
        {
            var result = await _discountService.AddProductsToDiscountAsync(id, productIds);
            return ApiResponseHelper.Success("Products added to discount successfully", result);
        }

        [HttpDelete("{id:int}/products/{productId:int}")]
        public async Task<IActionResult> RemoveProductFromDiscount(
            [FromRoute] int id,
            [FromRoute] int productId)
        {
            await _discountService.RemoveProductFromDiscountAsync(id, productId);
            return ApiResponseHelper.Success<object>("Product removed from discount successfully");
        }
    }
}
