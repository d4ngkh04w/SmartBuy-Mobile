using api.Models;

namespace api.Interfaces.Repositories
{
    public interface ICommentRepository
    {
        Task<Comment?> GetCommentByIdAsync(int id);
        Task<List<Comment>> GetCommentsByProductIdAsync(int productId, int page, int pageSize);
        Task<List<Comment>> GetRepliesByParentIdAsync(int parentId);
        Task<int> GetCommentsCountByProductIdAsync(int productId);
        Task<Comment> CreateCommentAsync(Comment comment);
        Task<Comment> UpdateCommentAsync(Comment comment);
        Task<bool> DeleteCommentAsync(int id);
        Task<double> GetProductAverageRatingAsync(int productId);
        Task<int> GetProductRatingCountAsync(int productId);
        Task<Dictionary<int, int>> GetProductRatingDistributionAsync(int productId);
        Task<bool> HasUserRatedProductAsync(Guid userId, int productId);
    }
}