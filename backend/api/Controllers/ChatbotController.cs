using api.DTOs.Chatbot;
using api.Helpers;
using api.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    public class ChatbotController : BaseController
    {
        private readonly IChatbotService _chatbotService;

        public ChatbotController(IChatbotService chatbotService)
        {
            _chatbotService = chatbotService;
        }

        [HttpPost("message")]
        [AllowAnonymous]
        public async Task<IActionResult> SendMessage([FromBody] ChatMessageDTO messageDTO)
        {
            var response = await _chatbotService.ProcessMessageAsync(messageDTO);
            return ApiResponseHelper.Success("Message processed successfully", response);
        }

        [HttpGet("quick-questions")]
        [AllowAnonymous]
        public IActionResult GetQuickQuestions()
        {
            var questions = _chatbotService.GetQuickQuestions();
            return ApiResponseHelper.Success("Quick questions retrieved successfully", questions);
        }

        [HttpPost("context")]
        [AllowAnonymous]
        public async Task<IActionResult> GetProductContext()
        {
            var context = await _chatbotService.GetProductContextAsync();
            return ApiResponseHelper.Success("Product context retrieved successfully", context);
        }
    }
}
