using api.Utils;

namespace api.DTOs.Comment
{
    public class CommentDTO
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string? UserAvatar { get; set; }
        public int ProductId { get; set; }
        public int? Rating { get; set; }
        public int? ParentId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string CreatedAtString => CreatedAt.ToRelativeTimeString();
        public string UpdatedAtString => UpdatedAt.ToRelativeTimeString();
        public bool IsDeleted { get; set; }
        public List<CommentDTO>? Replies { get; set; }
    }
}