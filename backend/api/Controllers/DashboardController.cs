using api.Helpers;
using api.Interfaces.Services;
using api.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [Authorize(AuthenticationSchemes = "admin", Roles = "admin")]
    public class DashboardController : BaseController
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }
        [HttpGet("top-products")]
        public async Task<IActionResult> GetTopProducts([FromQuery] DashboardDateRangeQuery query)
        {
            var topProducts = await _dashboardService.GetTopProductsAsync(query);
            return ApiResponseHelper.Success("Top products retrieved successfully", topProducts);
        }

        [HttpGet("order-statistics")]
        public async Task<IActionResult> GetOrderStatistics([FromQuery] DashboardDateRangeQuery query)
        {
            var statistics = await _dashboardService.GetOrderStatisticsAsync(query);
            return ApiResponseHelper.Success("Order statistics retrieved successfully", statistics);
        }
    }
}
