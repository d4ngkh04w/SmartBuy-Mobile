namespace api.DTOs.Dashboard
{
    public class OrderStatusDistributionDTO
    {
        public string Status { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal Percentage { get; set; }
    }
}