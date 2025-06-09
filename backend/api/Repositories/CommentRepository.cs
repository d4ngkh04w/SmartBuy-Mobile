using api.Database;
using api.Interfaces.Repositories;
using api.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Repositories
{
    public class CommentRepository : ICommentRepository
    {
        private readonly AppDBContext _db;

        public CommentRepository(AppDBContext db)
        {
            _db = db;
        }

        public async Task<Comment?> GetCommentByIdAsync(int id)
        {
            return await _db.Comments
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
        }

        public async Task<List<Comment>> GetCommentsByProductIdAsync(int productId, int page, int pageSize)
        {
            // Lấy các bình luận không có replies (ParentId = null) và phân trang
            return await _db.Comments
                .Include(c => c.User)
                .Where(c => c.ProductId == productId && c.ParentId == null && !c.IsDeleted)
                .OrderByDescending(c => c.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<Comment>> GetRepliesByParentIdAsync(int parentId)
        {
            return await _db.Comments
                .Include(c => c.User)
                .Where(c => c.ParentId == parentId && !c.IsDeleted)
                .OrderBy(c => c.CreatedAt)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<int> GetCommentsCountByProductIdAsync(int productId)
        {
            return await _db.Comments
                .Where(c => c.ProductId == productId && c.ParentId == null && !c.IsDeleted)
                .AsNoTracking()
                .CountAsync();
        }

        public async Task<Comment> CreateCommentAsync(Comment comment)
        {
            await _db.Comments.AddAsync(comment);
            await _db.SaveChangesAsync();

            return await _db.Comments
                .Include(c => c.User)
                .FirstAsync(c => c.Id == comment.Id);
        }

        public async Task<Comment> UpdateCommentAsync(Comment comment)
        {
            _db.Comments.Update(comment);
            await _db.SaveChangesAsync();
            return comment;
        }

        public async Task<bool> DeleteCommentAsync(int id)
        {
            var comment = await _db.Comments.FindAsync(id);
            if (comment == null)
                return false;

            comment.IsDeleted = true;
            comment.DeletedAt = DateTime.Now;

            _db.Comments.Update(comment);
            return await _db.SaveChangesAsync() > 0;
        }

        public async Task<double> GetProductAverageRatingAsync(int productId)
        {
            var ratings = await _db.Comments
                .Where(c => c.ProductId == productId && c.Rating.HasValue)
                .Select(c => c.Rating!.Value)
                .ToListAsync();

            if (!ratings.Any())
                return 0;

            return ratings.Average();
        }

        public async Task<int> GetProductRatingCountAsync(int productId)
        {
            return await _db.Comments
                .Where(c => c.ProductId == productId && c.Rating.HasValue && !c.IsDeleted)
                .CountAsync();
        }

        public async Task<Dictionary<int, int>> GetProductRatingDistributionAsync(int productId)
        {
            var distribution = new Dictionary<int, int>
            {
                { 1, 0 },
                { 2, 0 },
                { 3, 0 },
                { 4, 0 },
                { 5, 0 }
            };

            var ratingGroups = await _db.Comments
                .Where(c => c.ProductId == productId && c.Rating.HasValue && !c.IsDeleted)
                .GroupBy(c => c.Rating!.Value)
                .Select(g => new { Rating = g.Key, Count = g.Count() })
                .ToListAsync();

            foreach (var group in ratingGroups)
            {
                if (group.Rating >= 1 && group.Rating <= 5)
                {
                    distribution[group.Rating] = group.Count;
                }
            }

            return distribution;
        }

        public async Task<bool> HasUserRatedProductAsync(Guid userId, int productId)
        {
            return await _db.Comments
                .AnyAsync(c => c.UserId == userId && c.ProductId == productId
                       && c.Rating.HasValue && !c.IsDeleted && c.ParentId == null);
        }
    }
}