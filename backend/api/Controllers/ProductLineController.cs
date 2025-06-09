using api.DTOs.ProductLine;
using api.Helpers;
using api.Interfaces.Services;
using api.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [Authorize(AuthenticationSchemes = "admin", Roles = "admin")]
    public class ProductLineController : BaseController
    {
        private readonly IProductLineService _productLineService;

        public ProductLineController(IProductLineService productLineService)
        {
            _productLineService = productLineService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetProductLines([FromQuery] ProductLineQuery query)
        {
            var productLines = await _productLineService.GetProductLinesAsync(query);
            return ApiResponseHelper.Success("Product lines retrieved successfully", productLines);
        }

        [HttpGet("by-brand/{brandId:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetProductLinesByBrand([FromRoute] int brandId, [FromQuery] ProductLineQuery query)
        {
            query.BrandId = brandId;

            var productLines = await _productLineService.GetProductLinesAsync(query);
            return ApiResponseHelper.Success("Product lines for brand retrieved successfully", productLines);
        }

        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetProductLineById([FromRoute] int id, [FromQuery] ProductLineQuery query)
        {
            var productLine = await _productLineService.GetProductLineByIdAsync(id, query);
            return ApiResponseHelper.Success("Product line retrieved successfully", productLine);
        }

        [HttpPost]
        public async Task<IActionResult> CreateProductLine([FromForm] CreateProductLineDTO productLineDTO)
        {
            var productLine = await _productLineService.CreateProductLineAsync(productLineDTO);
            return ApiResponseHelper.Created("Product line created successfully", productLine);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateProductLine([FromRoute] int id, [FromForm] UpdateProductLineDTO productLineDTO)
        {
            var productLine = await _productLineService.UpdateProductLineAsync(id, productLineDTO);
            return ApiResponseHelper.Success("Product line updated successfully", productLine);
        }

        // [HttpDelete("{id:int}")]
        // public async Task<IActionResult> DeleteProductLine([FromRoute] int id)
        // {
        //     await _productLineService.DeleteProductLineAsync(id);
        //     return NoContent();
        // }

        [HttpPut("{id:int}/activate")]
        public async Task<IActionResult> ActivateProductLine([FromRoute] int id)
        {
            var productLine = await _productLineService.ActivateProductLineAsync(id);
            return ApiResponseHelper.Success("Product line activated successfully", productLine);
        }

        [HttpPut("{id:int}/deactivate")]
        public async Task<IActionResult> DeactivateProductLine([FromRoute] int id)
        {
            var productLine = await _productLineService.DeactivateProductLineAsync(id);
            return ApiResponseHelper.Success("Product line deactivated successfully", productLine);
        }
    }
}