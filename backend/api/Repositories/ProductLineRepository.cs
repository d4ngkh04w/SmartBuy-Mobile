using api.Database;
using api.Interfaces.Repositories;
using api.Models;
using api.Queries;
using Microsoft.EntityFrameworkCore;

namespace api.Repositories
{
    public class ProductLineRepository : IProductLineRepository
    {
        private readonly AppDBContext _db;

        public ProductLineRepository(AppDBContext db)
        {
            _db = db;
        }

        public async Task<ProductLine> CreateProductLineAsync(ProductLine productLine)
        {
            _db.ProductLines.Add(productLine);
            await _db.SaveChangesAsync();

            return (await _db.ProductLines
                .Include(pl => pl.Brand)
                .Include(pl => pl.Products)
                .FirstOrDefaultAsync(pl => pl.Id == productLine.Id))!;
        }

        public async Task<bool> ProductLineExistAsync(string name)
        {
            return await _db.ProductLines.AnyAsync(pl => pl.Name.ToLower() == name.ToLower());
        }

        public async Task<IEnumerable<ProductLine>> GetProductLinesAsync(ProductLineQuery query)
        {
            var productLinesQuery = _db.ProductLines.AsQueryable();

            if (query.IsActive.HasValue)
            {
                productLinesQuery = productLinesQuery.Where(pl => pl.IsActive == query.IsActive.Value);
            }

            if (query.BrandId.HasValue)
            {
                productLinesQuery = productLinesQuery.Where(pl => pl.BrandId == query.BrandId.Value);
            }

            if (query.IncludeProducts)
            {
                productLinesQuery = productLinesQuery
                                    .Include(pl => pl.Products)
                                        .ThenInclude(p => p.Colors)
                                            .ThenInclude(c => c.Images)
                                    .Include(pl => pl.Products)
                                        .ThenInclude(p => p.Discounts)
                                            .ThenInclude(d => d.Discount)
                                    .Include(pl => pl.Products)
                                        .ThenInclude(p => p.Detail);
            }

            productLinesQuery = query.SortBy.ToLower() switch
            {
                "id" => productLinesQuery.OrderBy(pl => pl.Id),
                "name" => productLinesQuery.OrderBy(pl => pl.Name),
                _ => productLinesQuery.OrderBy(pl => pl.Name),
            };

            if (query.IsDescending)
            {
                productLinesQuery = productLinesQuery.Reverse();
            }

            var query_with_brand = productLinesQuery.Include(pl => pl.Brand);

            return await query_with_brand.AsNoTracking().ToListAsync();
        }

        public async Task<ProductLine?> GetProductLineByIdAsync(int id, ProductLineQuery? query = null)
        {
            if (query == null)
            {
                return await _db.ProductLines.FindAsync(id);
            }

            var productLineQuery = _db.ProductLines.AsQueryable();

            if (query.IncludeProducts)
            {
                productLineQuery = productLineQuery
                                    .Include(pl => pl.Products)
                                        .ThenInclude(p => p.Colors)
                                            .ThenInclude(c => c.Images)
                                    .Include(pl => pl.Products)
                                        .ThenInclude(p => p.Discounts)
                                            .ThenInclude(d => d.Discount)
                                    .Include(pl => pl.Products)
                                        .ThenInclude(p => p.Detail);
            }

            return await productLineQuery
                .Include(pl => pl.Brand)
                .FirstOrDefaultAsync(pl => pl.Id == id);
        }

        public async Task<ProductLine> UpdateProductLineAsync(ProductLine productLine)
        {
            _db.ProductLines.Update(productLine);
            await _db.SaveChangesAsync();
            return productLine;
        }

        public async Task<List<ProductLine>> GetProductLinesByBrandIdAsync(int brandId)
        {
            return await _db.ProductLines
                .Include(pl => pl.Products)
                .Where(pl => pl.BrandId == brandId)
                .ToListAsync();
        }
    }
}