namespace api.DTOs.Chatbot
{
    public class ChatResponseDTO
    {
        public string Id { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public bool IsUser { get; set; } = false;
        public bool IsError { get; set; } = false;
        public List<string>? SuggestedActions { get; set; }
    }
}