using System.ComponentModel.DataAnnotations;

namespace api.DTOs.Comment
{
    public class UpdateCommentDTO
    {
        [MaxLength(1000, ErrorMessage = "Content cannot exceed 1000 characters")]
        public string? Content { get; set; } = string.Empty;

        public int? Rating { get; set; }
    }
}