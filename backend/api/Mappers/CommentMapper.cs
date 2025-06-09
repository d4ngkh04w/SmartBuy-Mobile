using api.DTOs.Comment;
using api.Models;
using api.Utils;

namespace api.Mappers
{
    public static class CommentMapper
    {
        public static CommentDTO ToCommentDTO(this Comment comment, bool includeReplies = false)
        {
            var dto = new CommentDTO
            {
                Id = comment.Id,
                Content = comment.IsDeleted ? "This comment has been deleted" : comment.Content ?? "",
                UserId = comment.UserId,
                UserName = comment.User?.Name ?? "Anonymous",
                UserAvatar = comment.User?.Avatar ?? "",
                ProductId = comment.ProductId,
                Rating = comment.Rating,
                ParentId = comment.ParentId,
                CreatedAt = DateTimeUtils.FormatDateTime(comment.CreatedAt),
                UpdatedAt = DateTimeUtils.FormatDateTime(comment.UpdatedAt),
                IsDeleted = comment.IsDeleted,
                Replies = null
            };

            return dto;
        }

        public static Comment ToCommentModel(this CreateCommentDTO dto, Guid userId)
        {
            return new Comment
            {
                Content = dto.Content?.Trim(),
                UserId = userId,
                ProductId = dto.ProductId,
                Rating = dto.Rating,
                ParentId = dto.ParentId,
                CreatedAt = DateTimeUtils.FormatDateTime(DateTime.Now),
                UpdatedAt = DateTimeUtils.FormatDateTime(DateTime.Now)
            };
        }
    }
}