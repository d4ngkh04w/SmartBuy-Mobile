using System.Text;
using System.Text.Json;
using api.DTOs.Chatbot;
using api.Helpers;
using api.Interfaces.Repositories;
using api.Interfaces.Services;

namespace api.Services
{    public class ChatbotService : IChatbotService
    {
        private readonly IProductRepository _productRepository;
        private readonly IBrandRepository _brandRepository;
        private readonly IProductLineRepository _productLineRepository;
        private readonly IDiscountRepository _discountRepository;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly GeminiChatbotService _geminiService;        public ChatbotService(
            IProductRepository productRepository,
            IBrandRepository brandRepository,
            IProductLineRepository productLineRepository,
            IDiscountRepository discountRepository,
            HttpClient httpClient,
            IConfiguration configuration,
            GeminiChatbotService geminiService)
        {
            _productRepository = productRepository;
            _brandRepository = brandRepository;
            _productLineRepository = productLineRepository;
            _discountRepository = discountRepository;
            _httpClient = httpClient;
            _configuration = configuration;
            _geminiService = geminiService;
        }

        public async Task<ChatResponseDTO> ProcessMessageAsync(ChatMessageDTO messageDTO)
        {
            try
            {
                // Lấy context sản phẩm
                var context = await GetProductContextAsync();

                // Tạo response từ OpenAI hoặc logic tùy chỉnh
                var content = await GenerateResponseAsync(messageDTO.Message, context);

                return new ChatResponseDTO
                {
                    Id = Guid.NewGuid().ToString(),
                    Content = content,
                    Timestamp = DateTime.UtcNow,
                    IsUser = false,
                    IsError = false,
                    SuggestedActions = GenerateSuggestedActions(messageDTO.Message)
                };
            }
            catch (Exception)
            {
                return new ChatResponseDTO
                {
                    Id = Guid.NewGuid().ToString(),
                    Content = "Xin lỗi, tôi đang gặp sự cố kỹ thuật. Vui lòng thử lại sau hoặc liên hệ support để được hỗ trợ.",
                    Timestamp = DateTime.UtcNow,
                    IsUser = false,
                    IsError = true
                };
            }
        }        public async Task<string> GenerateResponseAsync(string message, ProductContextDTO? context = null)
        {
            // Chỉ dùng Gemini
            var geminiKey = _configuration["Gemini:ApiKey"];
            if (!string.IsNullOrEmpty(geminiKey))
            {
                try
                {
                    var systemPrompt = BuildSystemPrompt(context);
                    var response = await _geminiService.GenerateResponseAsync(systemPrompt, message);
                    return response;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Gemini API Error: {ex.Message}");
                    // Fallback nếu Gemini fail
                    return GenerateFallbackResponse(message, context);
                }
            }

            // Fallback nếu không có Gemini key
            return GenerateFallbackResponse(message, context);
        }

        public List<string> GetQuickQuestions()
        {
            return new List<string>
            {
                "Làm sao để đặt hàng?",
                "Có những phương thức thanh toán nào?",
                "Có khuyến mãi gì hiện tại?",
                "Sản phẩm bán chạy nhất là gì?",
                "Có bao nhiêu nhãn hàng?",
                "IPhone hiện tại có bao nhiêu loại sản phẩm?",
            };
        }

        public async Task<ProductContextDTO> GetProductContextAsync()
        {
            try
            {
                var products = await _productRepository.GetAllAsync();
                var topProducts = products
                    .Where(p => p.IsActive)
                    .OrderBy(p => p.Name)
                    .Take(10)
                    .Select(p => new ProductSummaryDTO
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Price = p.SalePrice,
                        Brand = p.ProductLine?.Brand?.Name ?? "N/A",
                        Category = p.ProductLine?.Name ?? "N/A",
                        IsAvailable = p.IsActive,
                        ImageUrl = p.Colors.SelectMany(c => c.Images).FirstOrDefault()?.ImagePath
                    }).ToList();// Lấy danh sách brands
                var brands = await _brandRepository.GetBrandsAsync(new api.Queries.BrandQuery());
                var brandNames = brands.Where(b => b.IsActive).Select(b => b.Name).ToList();                // Lấy danh sách product lines (categories)
                var productLines = await _productLineRepository.GetProductLinesAsync(new api.Queries.ProductLineQuery());
                var categoryNames = productLines.Where(pl => pl.IsActive).Select(pl => pl.Name).ToList();                // Lấy khuyến mãi hiện tại
                var discounts = await _discountRepository.GetAllDiscountsAsync(); var currentPromotions = discounts.Where(d => d.StartDate <= DateTime.UtcNow && d.EndDate >= DateTime.UtcNow)
                    .Select(d => new PromotionDTO
                    {
                        Title = $"Giảm giá {(d.DiscountPercentage > 0 ? $"{d.DiscountPercentage}%" : $"{d.DiscountAmount:N0}đ")}",
                        Description = $"Áp dụng từ {d.StartDate:dd/MM/yyyy} đến {d.EndDate:dd/MM/yyyy}",
                        DiscountPercent = d.DiscountPercentage,
                        DiscountAmount = d.DiscountAmount
                    }).ToList();

                return new ProductContextDTO
                {
                    AvailableProducts = topProducts,
                    Brands = brandNames,
                    Categories = categoryNames,
                    CurrentPromotions = currentPromotions
                };
            }
            catch
            {
                return new ProductContextDTO
                {
                    AvailableProducts = new List<ProductSummaryDTO>(),
                    Brands = new List<string>(),
                    Categories = new List<string>(),
                    CurrentPromotions = new List<PromotionDTO>()
                };
            }
        }

        private string BuildSystemPrompt(ProductContextDTO? context)
        {
            var prompt = @"Bạn là SmartBuy Assistant, một trợ lý AI thông minh cho website thương mại điện tử SmartBuy. 

Nhiệm vụ của bạn:
1. Tư vấn sản phẩm và giúp khách hàng mua sắm
2. Hướng dẫn sử dụng website
3. Giải đáp thắc mắc về đơn hàng, thanh toán, vận chuyển
4. Hỗ trợ khách hàng một cách thân thiện và chuyên nghiệp

Quy tắc trả lời:
- Luôn trả lời bằng tiếng Việt
- Ngắn gọn, súc tích nhưng đầy đủ thông tin
- Thân thiện và lịch sự
- Nếu không biết thông tin cụ thể, hãy hướng dẫn khách hàng liên hệ support
- Gợi ý sản phẩm dựa trên nhu cầu khách hàng";

            if (context != null)
            {
                prompt += "\n\nThông tin sản phẩm hiện tại:\n";

                if (context.AvailableProducts?.Any() == true)
                {
                    prompt += "Sản phẩm nổi bật:\n";
                    foreach (var product in context.AvailableProducts.Take(5))
                    {
                        prompt += $"- {product.Name} ({product.Brand}) - {product.Price:N0}đ\n";
                    }
                }

                if (context.Brands?.Any() == true)
                {
                    prompt += $"\nThương hiệu có sẵn: {string.Join(", ", context.Brands)}\n";
                }

                if (context.CurrentPromotions?.Any() == true)
                {
                    prompt += "\nKhuyến mãi hiện tại:\n";
                    foreach (var promo in context.CurrentPromotions.Take(3))
                    {
                        prompt += $"- {promo.Title}: {promo.Description}\n";
                    }
                }
            }

            return prompt;
        }
        private string GenerateFallbackResponse(string message, ProductContextDTO? context)
        {
            var lowerMessage = message.ToLower();

            // Cửa hàng có bao nhiêu nhãn hàng/brand
            if ((lowerMessage.Contains("nhãn hàng") || lowerMessage.Contains("brand") || lowerMessage.Contains("thương hiệu")) &&
                lowerMessage.Contains("bao nhiêu"))
            {
                var brandCount = context?.Brands?.Count ?? 0;
                return $"🏷️ Hiện tại cửa hàng SmartBuy có **{brandCount} nhãn hàng/brand** khác nhau.\n\nCác thương hiệu bao gồm: {string.Join(", ", context?.Brands?.Take(5) ?? new List<string>())}{(brandCount > 5 ? "..." : "")}";
            }

            // Cửa hàng có bao nhiêu dòng sản phẩm/productline
            if ((lowerMessage.Contains("dòng sản phẩm") || lowerMessage.Contains("productline") || lowerMessage.Contains("danh mục")) &&
                lowerMessage.Contains("bao nhiêu"))
            {
                var categoryCount = context?.Categories?.Count ?? 0;
                return $"📂 Hiện tại cửa hàng SmartBuy có **{categoryCount} dòng sản phẩm/productline** khác nhau.\n\nCác danh mục bao gồm: {string.Join(", ", context?.Categories?.Take(5) ?? new List<string>())}{(categoryCount > 5 ? "..." : "")}";
            }            // iPhone hiện tại có bao nhiêu loại sản phẩm
            if (lowerMessage.Contains("iphone") &&
                (lowerMessage.Contains("bao nhiêu") || lowerMessage.Contains("có bao nhiêu")) &&
                (lowerMessage.Contains("loại") || lowerMessage.Contains("sản phẩm") || lowerMessage.Contains("model")))
            {
                var iphoneProducts = context?.AvailableProducts?.Where(p => p.Name.ToLower().Contains("iphone")).ToList();
                var iphoneCount = iphoneProducts?.Count ?? 0;

                if (iphoneCount > 0 && iphoneProducts != null)
                {
                    var iphoneText = $"📱 Hiện tại cửa hàng có **{iphoneCount} loại sản phẩm iPhone**:\n\n";
                    foreach (var iphone in iphoneProducts.Take(10))
                    {
                        iphoneText += $"• {iphone.Name} - {iphone.Price:N0}đ\n";
                    }
                    return iphoneText + "\nBạn quan tâm đến iPhone nào cụ thể không?";
                }
                return "📱 Hiện tại cửa hàng chưa có sản phẩm iPhone nào. Vui lòng quay lại sau!";
            }

            // Cho tôi những sản phẩm bán chạy nhất
            if (lowerMessage.Contains("sản phẩm") && (lowerMessage.Contains("bán chạy") || lowerMessage.Contains("phổ biến") || lowerMessage.Contains("nổi bật")))
            {
                var products = context?.AvailableProducts;
                if (products?.Any() == true)
                {
                    var productText = "🏆 Sản phẩm bán chạy nhất tại SmartBuy:\n\n";
                    foreach (var product in products.Take(5))
                    {
                        productText += $"📱 {product.Name}\n";
                        productText += $"   Thương hiệu: {product.Brand}\n";
                        productText += $"   Giá: {product.Price:N0}đ\n";
                        productText += $"   Danh mục: {product.Category}\n\n";
                    }
                    return productText + "Bạn muốn xem chi tiết sản phẩm nào không?";
                }
                return "Hiện tại chúng tôi đang cập nhật danh sách sản phẩm bán chạy. Vui lòng quay lại sau!";
            }

            // Có bao nhiêu cách thanh toán
            if ((lowerMessage.Contains("bao nhiêu") || lowerMessage.Contains("có bao nhiêu")) &&
                (lowerMessage.Contains("cách thanh toán") || lowerMessage.Contains("phương thức thanh toán") || lowerMessage.Contains("thanh toán")))
            {
                return "💳 SmartBuy hỗ trợ **4 phương thức thanh toán**:\n\n" +
                       "1️⃣ Thanh toán khi nhận hàng (COD)\n" +
                       "2️⃣ Chuyển khoản ngân hàng\n" +
                       "3️⃣ Ví điện tử (Momo, ZaloPay)\n" +
                       "4️⃣ Thẻ tín dụng/ghi nợ\n\n" +
                       "Tất cả đều an toàn và bảo mật! Bạn muốn biết thêm chi tiết về phương thức nào không?";
            }

            if (lowerMessage.Contains("đặt hàng") || lowerMessage.Contains("mua"))
            {
                return "Để đặt hàng, bạn có thể:\n1. Thêm sản phẩm vào giỏ hàng\n2. Chọn 'Thanh toán'\n3. Điền thông tin giao hàng\n4. Chọn phương thức thanh toán\n5. Xác nhận đơn hàng\n\nBạn có cần hỗ trợ gì thêm không?";
            }

            if (lowerMessage.Contains("giao hàng") || lowerMessage.Contains("ship"))
            {
                return "Thời gian giao hàng:\n- Nội thành: 1-2 ngày\n- Ngoại thành: 2-3 ngày\n- Tỉnh xa: 3-5 ngày\n\nPhí ship từ 30.000đ tùy theo khu vực. Miễn phí ship cho đơn hàng từ 500.000đ.";
            }

            if (lowerMessage.Contains("thanh toán"))
            {
                return "SmartBuy hỗ trợ các phương thức thanh toán:\n- Thanh toán khi nhận hàng (COD)\n- Chuyển khoản ngân hàng\n- Ví điện tử (Momo, ZaloPay)\n- Thẻ tín dụng/ghi nợ\n\nTất cả đều an toàn và bảo mật.";
            }

            if (lowerMessage.Contains("khuyến mãi") || lowerMessage.Contains("giảm giá"))
            {
                var promotions = context?.CurrentPromotions;
                if (promotions?.Any() == true)
                {
                    var promoText = "Khuyến mãi hiện tại:\n";
                    foreach (var promo in promotions.Take(3))
                    {
                        promoText += $"🎁 {promo.Title}: {promo.Description}\n";
                    }
                    return promoText + "\nHãy nhanh tay để không bỏ lỡ!";
                }
                return "Hiện tại SmartBuy có nhiều chương trình khuyến mãi hấp dẫn. Vui lòng kiểm tra trang chủ để xem chi tiết các ưu đãi mới nhất!";
            }

            // Response mặc định
            return "Cảm ơn bạn đã liên hệ với SmartBuy! Tôi có thể giúp bạn:\n\n" +
                   "🛒 Tư vấn sản phẩm\n" +
                   "📦 Hướng dẫn đặt hàng\n" +
                   "🚚 Thông tin giao hàng\n" +
                   "💳 Phương thức thanh toán\n" +
                   "🔄 Chính sách đổi trả\n\n" +
                   "Bạn cần hỗ trợ gì cụ thể không?";
        }

        private List<string> GenerateSuggestedActions(string message)
        {
            var lowerMessage = message.ToLower();
            var suggestions = new List<string>();

            if (lowerMessage.Contains("sản phẩm") || lowerMessage.Contains("tư vấn"))
            {
                suggestions.AddRange(["Xem sản phẩm bán chạy", "Tìm theo thương hiệu", "So sánh giá"]);
            }
            else if (lowerMessage.Contains("đặt hàng"))
            {
                suggestions.AddRange(["Hướng dẫn thanh toán", "Chính sách giao hàng", "Theo dõi đơn hàng"]);
            }
            else
            {
                suggestions.AddRange(["Sản phẩm mới", "Khuyến mãi hot", "Hỗ trợ đặt hàng"]);
            }

            return suggestions.Take(3).ToList();
        }
      
    }
}
