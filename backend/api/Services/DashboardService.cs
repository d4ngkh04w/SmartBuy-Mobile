using api.DTOs.Dashboard;
using api.Interfaces.Repositories;
using api.Interfaces.Services;
using api.Queries;

namespace api.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IDashboardRepository _dashboardRepository;

        public DashboardService(IDashboardRepository dashboardRepository)
        {
            _dashboardRepository = dashboardRepository;
        }

        public async Task<List<TopProductDTO>> GetTopProductsAsync(DashboardDateRangeQuery query)
        {
            var data = await _dashboardRepository.GetTopProductsAsync(
                query.StartDate,
                query.EndDate,
                query.SortBy ?? "quantity"
            );

            return data;
        }
        public async Task<OrderReportDTO> GetOrderStatisticsAsync(DashboardDateRangeQuery query)
        {
            var statistics = await _dashboardRepository.GetOrderStatisticsAsync(
                query.StartDate,
                query.EndDate
            );

            var statusDistribution = await _dashboardRepository.GetOrderStatusDistributionAsync(
                query.StartDate,
                query.EndDate
            );

            var paymentMethods = await _dashboardRepository.GetPaymentMethodStatsAsync(
                query.StartDate,
                query.EndDate
            );

            var revenueChartData = await _dashboardRepository.GetRevenueChartDataAsync(
                query.StartDate,
                query.EndDate,
                query.Period ?? "month"
            );

            return new OrderReportDTO
            {
                Statistics = statistics,
                StatusDistribution = statusDistribution,
                PaymentMethods = paymentMethods,
                RevenueChartData = revenueChartData
            };
        }
    }
}
