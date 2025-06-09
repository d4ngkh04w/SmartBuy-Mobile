namespace api.DTOs.Dashboard
{
    public class PaymentMethodStatDTO
    {
        public string Method { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal Percentage { get; set; }
    }
}