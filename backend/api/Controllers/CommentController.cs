using api.DTOs.Comment;
using api.Exceptions;
using api.Helpers;
using api.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [Authorize(AuthenticationSchemes = "smart", Roles = "admin,user")]
    public class CommentController : BaseController
    {
        private readonly ICommentService _commentService;

        public CommentController(ICommentService commentService)
        {
            _commentService = commentService;
        }

        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCommentById(int id)
        {
            var comment = await _commentService.GetCommentByIdAsync(id);
            return ApiResponseHelper.Success("Comment retrieved successfully", comment);
        }

        [HttpGet("product/{productId:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCommentsByProductId(int productId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var comments = await _commentService.GetCommentsByProductIdAsync(productId, page, pageSize);
            return ApiResponseHelper.Success("Comments retrieved successfully", comments);
        }

        [HttpGet("product/{productId:int}/rating")]
        [AllowAnonymous]
        public async Task<IActionResult> GetProductRatingStats(int productId)
        {
            var ratingStats = await _commentService.GetProductRatingStatsAsync(productId);
            return ApiResponseHelper.Success("Product rating statistics retrieved successfully", ratingStats);
        }

        [HttpPost]
        public async Task<IActionResult> CreateComment([FromBody] CreateCommentDTO commentDTO)
        {
            var userId = HttpContextHelper.CurrentUserId;
            if (userId == Guid.Empty)
                throw new UnauthorizedException();

            var comment = await _commentService.CreateCommentAsync(commentDTO, userId);
            return ApiResponseHelper.Created("Comment created successfully", comment);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateComment(int id, [FromBody] UpdateCommentDTO commentDTO)
        {
            var userId = HttpContextHelper.CurrentUserId;
            if (userId == Guid.Empty)
                throw new UnauthorizedException();

            var comment = await _commentService.UpdateCommentAsync(id, commentDTO, userId);

            return ApiResponseHelper.Success("Comment updated successfully", comment);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteComment(int id)
        {
            var userId = HttpContextHelper.CurrentUserId;
            if (userId == Guid.Empty)
                throw new UnauthorizedException();

            await _commentService.DeleteCommentAsync(id, userId);

            return NoContent();
        }

        [HttpPost("{id:int}/reaction")]
        public async Task<IActionResult> AddReaction(int id, [FromBody] CommentReactionDTO reactionDTO)
        {
            var userId = HttpContextHelper.CurrentUserId;
            if (userId == Guid.Empty)
                throw new UnauthorizedException();

            var comment = await _commentService.AddReactionAsync(id, reactionDTO, userId);

            return ApiResponseHelper.Success("Comment reaction added successfully", comment);
        }
    }
}