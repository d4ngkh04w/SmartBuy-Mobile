using api.Database;
using api.Interfaces.Repositories;
using api.Models;
using api.Queries;
using Microsoft.EntityFrameworkCore;

namespace api.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly AppDBContext _db;

        public ProductRepository(AppDBContext context)
        {
            _db = context;
        }
        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await _db.Products
                .Include(p => p.ProductLine)
                .Include(p => p.Colors)
                    .ThenInclude(c => c.Images)
                .Include(p => p.Discounts)
                    .ThenInclude(d => d.Discount)
                .Include(p => p.Detail)
                .AsNoTracking()
                .ToListAsync();
        }
        public async Task<Product?> GetByIdAsync(int id)
        {
            return await _db.Products
                .Include(p => p.ProductLine)
                .Include(p => p.Colors)
                    .ThenInclude(c => c.Images)
                .Include(p => p.Discounts)
                    .ThenInclude(d => d.Discount)
                .Include(p => p.Detail)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Product> CreateAsync(Product product)
        {
            await _db.Products.AddAsync(product);
            await _db.SaveChangesAsync();
            return product;
        }

        public async Task<Product> UpdateAsync(Product product)
        {
            _db.Products.Update(product);
            await _db.SaveChangesAsync();
            return product;
        }

        public async Task<bool> DeleteAsync(Product product)
        {
            if (product == null)
                return false;

            product.ManuallyDeactivated = true;
            product.UpdatedAt = DateTime.Now;

            _db.Products.Update(product);
            return await _db.SaveChangesAsync() > 0;
        }

        public async Task<bool> ExistsByNameAsync(string name)
        {
            return await _db.Products.AnyAsync(p => p.Name.ToLower() == name.ToLower());
        }

        public async Task<(List<Product> Items, int TotalItems)> GetPagedProductsAsync(ProductQuery productQuery)
        {
            var baseQuery = _db.Products.AsQueryable();

            if (productQuery.IsActive.HasValue)
            {
                baseQuery = baseQuery.Where(p => p.IsActive == productQuery.IsActive.Value);
            }

            if (!string.IsNullOrWhiteSpace(productQuery.Search))
            {
                string lowerKey = productQuery.Search.Trim().ToLower();
                baseQuery = baseQuery.Where(p => p.Name.ToLower().Contains(lowerKey));
            }

            if (productQuery.ProductLineId.HasValue)
            {
                baseQuery = baseQuery.Where(p => p.ProductLineId == productQuery.ProductLineId.Value);
            }

            // Optimize brand filtering with index usage
            if (!string.IsNullOrWhiteSpace(productQuery.BrandName))
            {
                string lowerBrand = productQuery.BrandName.Trim().ToLower();
                baseQuery = baseQuery.Where(p =>
                    p.ProductLine!.Brand!.Name.ToLower().Contains(lowerBrand));
            }

            if (productQuery.MinPrice.HasValue)
            {
                baseQuery = baseQuery.Where(p => p.SalePrice >= productQuery.MinPrice.Value);
            }

            if (productQuery.MaxPrice.HasValue)
            {
                baseQuery = baseQuery.Where(p => p.SalePrice <= productQuery.MaxPrice.Value);
            }

            // Apply sorting
            baseQuery = productQuery.SortBy?.ToLower() switch
            {
                "oldest" => baseQuery.OrderBy(p => p.CreatedAt),
                "priceinc" => baseQuery.OrderBy(p => p.SalePrice),
                "pricedesc" => baseQuery.OrderByDescending(p => p.SalePrice),
                "bestselling" => baseQuery.OrderByDescending(p => p.Sold),
                _ => baseQuery.OrderByDescending(p => p.CreatedAt) // newest - default
            };

            // Get total count before including related data (more efficient)
            var totalItems = await baseQuery.CountAsync();

            var items = await baseQuery
                .Skip(((productQuery.Page ?? 1) - 1) * (productQuery.PageSize ?? 10))
                .Take(productQuery.PageSize ?? 10)
                .Include(p => p.ProductLine)
                .Include(p => p.Colors)
                    .ThenInclude(c => c.Images)
                .Include(p => p.Discounts)
                    .ThenInclude(pd => pd.Discount)
                .AsNoTracking()
                .ToListAsync();

            return (items, totalItems);
        }

        public async Task<List<Product>> GetProductsByProductLineIdAsync(int productLineId)
        {
            return await _db.Products
                .Where(p => p.ProductLineId == productLineId)
                .ToListAsync();
        }

        public async Task<ProductColor> AddColorAsync(ProductColor color)
        {
            await _db.Colors.AddAsync(color);
            await _db.SaveChangesAsync();
            return color;
        }

        public async Task<ProductImage> AddImageAsync(ProductImage image)
        {
            await _db.ProductImages.AddAsync(image);
            await _db.SaveChangesAsync();
            return image;
        }

        public async Task<ProductColor?> GetProductColorAsync(int productId, int colorId)
        {
            return await _db.Colors
                .Where(c => c.ProductId == productId && c.Id == colorId)
                .Include(c => c.Images)
                .FirstOrDefaultAsync();
        }
        public async Task<ProductColor> UpdateColorAsync(ProductColor color)
        {
            _db.Colors.Update(color);
            await _db.SaveChangesAsync();
            return color;
        }

        public async Task DeleteColorAsync(ProductColor color)
        {
            _db.Colors.Remove(color);
            await _db.SaveChangesAsync();
        }
        public async Task<IEnumerable<ProductImage>> AddImagesAsync(IEnumerable<ProductImage> images)
        {
            _db.ProductImages.AddRange(images);
            await _db.SaveChangesAsync();
            return images;
        }

        // Bulk operations for better performance
        public async Task<bool> UpdateProductsActiveStatusAsync(List<int> productIds, bool isActive)
        {
            if (productIds == null || productIds.Count == 0) return false;

            var products = await _db.Products
                .Where(p => productIds.Contains(p.Id))
                .ToListAsync();

            foreach (var product in products)
            {
                product.IsActive = isActive;
                product.UpdatedAt = DateTime.Now;
            }

            return await _db.SaveChangesAsync() > 0;
        }
    }
}