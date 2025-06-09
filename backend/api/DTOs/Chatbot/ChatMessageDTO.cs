namespace api.DTOs.Chatbot
{
    public class ChatMessageDTO
    {
        public string Message { get; set; } = string.Empty;
        public string? SessionId { get; set; }
        public Dictionary<string, object>? Context { get; set; }
    }
}