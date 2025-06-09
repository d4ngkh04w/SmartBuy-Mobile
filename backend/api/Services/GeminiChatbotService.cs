using System.Text;
using System.Text.Json;
using api.DTOs.Chatbot;

namespace api.Services
{
    public class GeminiChatbotService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public GeminiChatbotService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["Gemini:ApiKey"] ?? throw new InvalidOperationException("Gemini API key not found");
        }

        public async Task<string> GenerateResponseAsync(string systemPrompt, string userMessage)
        {
            try
            {
                var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash-latest:generateContent?key={_apiKey}";
                
                var requestBody = new
                {
                    contents = new[]
                    {
                        new
                        {
                            parts = new[]
                            {
                                new { text = $"{systemPrompt}\n\nUser: {userMessage}" }
                            }
                        }
                    },
                    generationConfig = new
                    {
                        temperature = 0.8,
                        maxOutputTokens = 500
                    }
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    return ExtractContentFromGeminiResponse(responseContent);
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Gemini API Error: {response.StatusCode} - {errorContent}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error calling Gemini API: {ex.Message}", ex);
            }
        }

        private string ExtractContentFromGeminiResponse(string responseJson)
        {
            try
            {
                using var document = JsonDocument.Parse(responseJson);
                var candidates = document.RootElement.GetProperty("candidates");
                
                if (candidates.GetArrayLength() > 0)
                {
                    var firstCandidate = candidates[0];
                    var content = firstCandidate.GetProperty("content");
                    var parts = content.GetProperty("parts");
                    
                    if (parts.GetArrayLength() > 0)
                    {
                        var text = parts[0].GetProperty("text").GetString();
                        return text ?? "Xin lỗi, tôi không thể trả lời câu hỏi này.";
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error parsing Gemini response: {ex.Message}", ex);
            }

            return "Xin lỗi, tôi không thể trả lời câu hỏi này lúc này.";
        }
    }
}
