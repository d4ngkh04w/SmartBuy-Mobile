namespace api.DTOs.Dashboard
{
    public class OrderStatisticsDTO
    {
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AvgOrderValue { get; set; }
        public decimal CompletionRate { get; set; }
        public decimal OrderChange { get; set; }
        public decimal RevenueChange { get; set; }
        public decimal AvgChange { get; set; }
        public decimal CompletionChange { get; set; }
    }
}
