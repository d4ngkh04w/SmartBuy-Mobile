namespace api.DTOs.Chatbot
{
    public class ProductContextDTO
    {
        public List<ProductSummaryDTO>? AvailableProducts { get; set; }
        public List<string>? Brands { get; set; }
        public List<string>? Categories { get; set; }
        public List<PromotionDTO>? CurrentPromotions { get; set; }
    }
    public class ProductSummaryDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Brand { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public bool IsAvailable { get; set; }
        public string? ImageUrl { get; set; }
    }
}