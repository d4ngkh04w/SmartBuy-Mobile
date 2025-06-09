using api.DTOs.Brand;
using api.Helpers;
using api.Interfaces.Services;
using api.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [Authorize(AuthenticationSchemes = "admin", Roles = "admin")]
    public class BrandController : BaseController
    {
        private readonly IBrandService _brandService;

        public BrandController(IBrandService brandService)
        {
            _brandService = brandService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetBrands([FromQuery] BrandQuery query)
        {
            var brands = await _brandService.GetBrandsAsync(query);
            return ApiResponseHelper.Success("Brands retrieved successfully", brands);
        }

        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetBrandById([FromRoute] int id, [FromQuery] BrandQuery query)
        {
            var brand = await _brandService.GetBrandByIdAsync(id, query);
            return ApiResponseHelper.Success("Brand retrieved successfully", brand);
        }

        [HttpPost]
        public async Task<IActionResult> CreateBrand([FromForm] CreateBrandDTO brandDTO)
        {
            var brand = await _brandService.CreateBrandAsync(brandDTO);
            return ApiResponseHelper.Created("Brand created successfully", brand);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateBrand([FromRoute] int id, [FromForm] UpdateBrandDTO brandDTO)
        {
            var brand = await _brandService.UpdateBrandAsync(id, brandDTO);
            return ApiResponseHelper.Success("Brand updated successfully", brand);
        }

        [HttpPut("{id:int}/activate")]
        public async Task<IActionResult> ActivateBrand([FromRoute] int id)
        {
            var brand = await _brandService.ActivateBrandAsync(id);
            return ApiResponseHelper.Success("Brand activated successfully", brand);
        }

        [HttpPut("{id:int}/deactivate")]
        public async Task<IActionResult> DeactivateBrand([FromRoute] int id)
        {
            var brand = await _brandService.DeactivateBrandAsync(id);
            return ApiResponseHelper.Success("Brand deactivated successfully", brand);
        }
    }
}