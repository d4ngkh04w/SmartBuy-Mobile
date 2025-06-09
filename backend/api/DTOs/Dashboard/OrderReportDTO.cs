namespace api.DTOs.Dashboard
{
    public class OrderReportDTO
    {
        public OrderStatisticsDTO Statistics { get; set; } = new();
        public List<OrderStatusDistributionDTO> StatusDistribution { get; set; } = new();
        public List<PaymentMethodStatDTO> PaymentMethods { get; set; } = new();
        public List<RevenueChartDTO> RevenueChartData { get; set; } = new();
    }
}