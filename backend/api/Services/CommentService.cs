using api.DTOs.Comment;
using api.Exceptions;
using api.Helpers;
using api.Interfaces.Repositories;
using api.Interfaces.Services;
using api.Mappers;

namespace api.Services
{
    public class CommentService : ICommentService
    {
        private readonly ICommentRepository _commentRepository;
        private readonly IProductRepository _productRepository;
        private readonly IOrderService _orderService;
        private readonly ICacheService _cacheService;
        private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(5);

        public CommentService(ICommentRepository commentRepository, IProductRepository productRepository, IOrderService orderService, ICacheService cacheService)
        {
            _commentRepository = commentRepository;
            _productRepository = productRepository;
            _orderService = orderService;
            _cacheService = cacheService;
        }

        public async Task<CommentDTO> GetCommentByIdAsync(int id)
        {
            string cacheKey = CacheKeyManager.GetCommentKey(id);

            if (_cacheService.TryGetValue(cacheKey, out CommentDTO? cachedComment) && cachedComment != null)
            {
                return cachedComment;
            }

            var comment = await _commentRepository.GetCommentByIdAsync(id) ?? throw new NotFoundException("Comment not found");

            // Lấy những phản hồi của bình luận
            var replies = await _commentRepository.GetRepliesByParentIdAsync(id);
            var commentDto = comment.ToCommentDTO();
            commentDto.Replies = replies.Select(r => r.ToCommentDTO()).ToList();

            _cacheService.Set(cacheKey, commentDto, _cacheDuration);

            return commentDto;
        }

        public async Task<CommentResponseDTO> GetCommentsByProductIdAsync(int productId, int page, int pageSize)
        {
            string cacheKey = CacheKeyManager.GetCommentsByProductIdKey(productId, page, pageSize);

            if (_cacheService.TryGetValue(cacheKey, out CommentResponseDTO? cachedResponse) && cachedResponse != null)
            {
                return cachedResponse;
            }

            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null)
            {
                throw new NotFoundException("Product not found");
            }
            var comments = await _commentRepository.GetCommentsByProductIdAsync(productId, page, pageSize);
            // Tổng số bình luận cho sản phẩm
            var totalComments = await _commentRepository.GetCommentsCountByProductIdAsync(productId);
            // Tổng số trang
            var totalPages = (int)Math.Ceiling(totalComments / (double)pageSize);

            // Lấy các reply của từng comment
            var commentDtos = new List<CommentDTO>();
            foreach (var comment in comments)
            {
                var dto = comment.ToCommentDTO();
                var replies = await _commentRepository.GetRepliesByParentIdAsync(comment.Id);
                dto.Replies = replies.Select(r => r.ToCommentDTO()).ToList();
                commentDtos.Add(dto);
            }

            var result = new CommentResponseDTO
            {
                Comments = commentDtos,
                TotalItems = totalComments,
                CurrentPage = page,
                PageSize = pageSize,
                TotalPages = totalPages
            };

            _cacheService.Set(cacheKey, result, _cacheDuration);

            return result;
        }

        public async Task<CommentDTO> CreateCommentAsync(CreateCommentDTO commentDTO, Guid userId)
        {
            _ = await _productRepository.GetByIdAsync(commentDTO.ProductId) ?? throw new NotFoundException("Product not found");

            // Kiểm tra bình luận phải có nội dung hoặc đánh giá sao
            if (string.IsNullOrWhiteSpace(commentDTO.Content) && !commentDTO.Rating.HasValue)
            {
                throw new BadRequestException("Comment must contain content or rating");
            }

            if (commentDTO.ParentId.HasValue)
            {
                var parentComment = await _commentRepository.GetCommentByIdAsync(commentDTO.ParentId.Value) ?? throw new NotFoundException("Parent comment not found");

                // Kiểm tra xem bình luận cha có thuộc về sản phẩm không
                if (parentComment.ProductId != commentDTO.ProductId)
                {
                    throw new BadRequestException("Parent comment is not associated with the specified product");
                }

                // Rating chỉ cho bình luận gốc
                if (commentDTO.Rating.HasValue)
                {
                    throw new BadRequestException("Rating can only be provided in top-level comments");
                }

                // Phản hồi bình luận phải có nội dung
                if (string.IsNullOrWhiteSpace(commentDTO.Content))
                {
                    throw new BadRequestException("Reply comment must contain content");
                }
            }
            // Kiểm tra nếu có đánh giá sao thì người dùng đã mua sản phẩm chưa
            if (commentDTO.Rating.HasValue)
            {
                bool hasPurchased = await _orderService.HasUserPurchasedProductAsync(userId, commentDTO.ProductId);
                if (!hasPurchased)
                {
                    throw new BadRequestException("You can only rate products that you have purchased");
                }

                // Kiểm tra xem người dùng đã đánh giá sao cho sản phẩm này chưa
                bool hasAlreadyRated = await _commentRepository.HasUserRatedProductAsync(userId, commentDTO.ProductId);
                if (hasAlreadyRated)
                {
                    throw new BadRequestException("You have already rated this product. You can only rate a product once");
                }

                if (commentDTO.Rating < 1 || commentDTO.Rating > 5)
                {
                    throw new BadRequestException("Rating must be between 1 and 5");
                }
            }

            var comment = commentDTO.ToCommentModel(userId);
            comment.Content = comment.Content?.Trim();
            var createdComment = await _commentRepository.CreateCommentAsync(comment);

            var commentDto = createdComment.ToCommentDTO();

            _cacheService.RemoveCommentsByProductCache(commentDTO.ProductId);
            if (commentDTO.Rating.HasValue)
            {
                string ratingCacheKey = CacheKeyManager.GetProductAverageRatingKey(commentDTO.ProductId);
                string ratingStatsCacheKey = CacheKeyManager.GetProductRatingStatsKey(commentDTO.ProductId);
                _cacheService.Remove(ratingCacheKey);
                _cacheService.Remove(ratingStatsCacheKey);
                _cacheService.RemoveProductCache(commentDTO.ProductId);
                _cacheService.RemoveAllProductsCache();
            }

            if (commentDTO.ParentId.HasValue)
            {
                _cacheService.RemoveCommentCache(commentDTO.ParentId.Value);
            }

            return commentDto;
        }

        public async Task<CommentDTO> UpdateCommentAsync(int id, UpdateCommentDTO commentDTO, Guid userId)
        {
            var comment = await _commentRepository.GetCommentByIdAsync(id) ?? throw new NotFoundException("Comment not found");
            if (comment.UserId != userId)
            {
                throw new UnauthorizedAccessException("You are not authorized to update this comment");
            }
            if (comment.IsDeleted)
            {
                throw new BadRequestException("Cannot update a deleted comment");
            }

            if (string.IsNullOrWhiteSpace(commentDTO.Content) && !commentDTO.Rating.HasValue)
            {
                throw new BadRequestException("Comment must contain content or rating");
            }

            if (comment.ParentId.HasValue && commentDTO.Rating.HasValue)
            {
                throw new BadRequestException("Rating can only be provided in top-level comments");
            }

            if (comment.ParentId.HasValue && string.IsNullOrWhiteSpace(commentDTO.Content))
            {
                throw new BadRequestException("Reply comment must contain content");
            }

            if (commentDTO.Rating.HasValue)
            {
                bool hasPurchased = await _orderService.HasUserPurchasedProductAsync(userId, comment.ProductId);
                if (!hasPurchased)
                {
                    throw new BadRequestException("You can only rate products that you have purchased");
                }

                // Nếu bình luận chưa có đánh giá và người dùng muốn thêm đánh giá
                if (!comment.Rating.HasValue)
                {
                    // Kiểm tra xem người dùng đã đánh giá sản phẩm này trong bình luận khác chưa
                    bool hasAlreadyRated = await _commentRepository.HasUserRatedProductAsync(userId, comment.ProductId);
                    if (hasAlreadyRated)
                    {
                        throw new BadRequestException("You have already rated this product in another comment. You can only rate a product once");
                    }
                }

                if (commentDTO.Rating < 1 || commentDTO.Rating > 5)
                {
                    throw new BadRequestException("Rating must be between 1 and 5");
                }
            }
            bool ratingChanged = commentDTO.Rating.HasValue && comment.Rating != commentDTO.Rating.Value;

            comment.Content = commentDTO.Content?.Trim();
            comment.Rating = commentDTO.Rating ?? comment.Rating;
            comment.UpdatedAt = DateTime.Now;

            var commentRes = await _commentRepository.UpdateCommentAsync(comment);

            _cacheService.RemoveCommentCache(id);
            _cacheService.RemoveCommentsByProductCache(comment.ProductId);

            if (comment.ParentId.HasValue)
            {
                _cacheService.RemoveCommentCache(comment.ParentId.Value);
            }
            if (ratingChanged)
            {
                string ratingCacheKey = CacheKeyManager.GetProductAverageRatingKey(comment.ProductId);
                string ratingStatsCacheKey = CacheKeyManager.GetProductRatingStatsKey(comment.ProductId);
                _cacheService.Remove(ratingCacheKey);
                _cacheService.Remove(ratingStatsCacheKey);
                _cacheService.RemoveProductCache(comment.ProductId);
                _cacheService.RemoveAllProductsCache();
            }

            return commentRes.ToCommentDTO();
        }

        public async Task DeleteCommentAsync(int id, Guid userId)
        {
            var comment = await _commentRepository.GetCommentByIdAsync(id) ?? throw new NotFoundException("Comment not found");
            if (comment.UserId != userId)
            {
                throw new UnauthorizedAccessException("You are not authorized to delete this comment");
            }

            bool deleted = await _commentRepository.DeleteCommentAsync(id);
            if (!deleted)
            {
                throw new ServerException("Failed to delete comment");
            }

            _cacheService.RemoveCommentCache(id);
            _cacheService.RemoveCommentsByProductCache(comment.ProductId);

            if (comment.ParentId.HasValue)
            {
                _cacheService.RemoveCommentCache(comment.ParentId.Value);
            }
            if (comment.Rating.HasValue)
            {
                string ratingCacheKey = CacheKeyManager.GetProductAverageRatingKey(comment.ProductId);
                string ratingStatsCacheKey = CacheKeyManager.GetProductRatingStatsKey(comment.ProductId);
                _cacheService.Remove(ratingCacheKey);
                _cacheService.Remove(ratingStatsCacheKey);
                _cacheService.RemoveProductCache(comment.ProductId);
                _cacheService.RemoveAllProductsCache();
            }
        }

        public async Task<ProductRatingStatsDTO> GetProductRatingStatsAsync(int productId)
        {
            string cacheKey = CacheKeyManager.GetProductRatingStatsKey(productId);

            // Try to get from cache first
            if (_cacheService.TryGetValue(cacheKey, out ProductRatingStatsDTO? cachedStats) && cachedStats != null)
            {
                return cachedStats;
            }

            // Check if product exists
            _ = await _productRepository.GetByIdAsync(productId) ?? throw new NotFoundException("Product not found");

            // Get all necessary data
            double averageRating = await _commentRepository.GetProductAverageRatingAsync(productId);
            int totalRatings = await _commentRepository.GetProductRatingCountAsync(productId);
            Dictionary<int, int> ratingDistribution = await _commentRepository.GetProductRatingDistributionAsync(productId);

            // Create rating distribution details with percentage calculation
            var ratingDetails = new Dictionary<int, RatingDetailDTO>();
            foreach (var kvp in ratingDistribution)
            {
                double percentage = totalRatings > 0 ? (double)kvp.Value / totalRatings * 100 : 0;
                ratingDetails[kvp.Key] = new RatingDetailDTO
                {
                    Count = kvp.Value,
                    Percentage = percentage
                };
            }

            // Create the stats object
            var stats = new ProductRatingStatsDTO
            {
                AverageRating = averageRating,
                TotalRatings = totalRatings,
                RatingDistribution = ratingDetails
            };

            // Store in cache
            _cacheService.Set(cacheKey, stats, _cacheDuration);

            return stats;
        }

        public async Task<bool> HasUserRatedProductAsync(int productId, Guid userId)
        {
            _ = await _productRepository.GetByIdAsync(productId) ?? throw new NotFoundException("Product not found");

            bool hasRated = await _commentRepository.HasUserRatedProductAsync(userId, productId);

            return hasRated;
        }
    }
}