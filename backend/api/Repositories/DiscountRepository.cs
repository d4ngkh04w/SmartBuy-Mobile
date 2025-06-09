using api.Database;
using api.Interfaces.Repositories;
using api.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Repositories
{
    public class DiscountRepository : IDiscountRepository
    {
        private readonly AppDBContext _context;

        public DiscountRepository(AppDBContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Discount>> GetAllDiscountsAsync()
        {
            return await _context.Discounts
                .OrderByDescending(d => d.StartDate)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Discount?> GetDiscountByIdAsync(int id)
        {
            return await _context.Discounts.FindAsync(id);
        }

        public async Task<Discount> CreateDiscountAsync(Discount discount)
        {
            _context.Discounts.Add(discount);
            await _context.SaveChangesAsync();
            return discount;
        }

        public async Task<Discount?> UpdateDiscountAsync(int id, Discount updatedDiscount)
        {
            var discount = await _context.Discounts.FindAsync(id);
            if (discount == null)
            {
                return null;
            }

            discount.DiscountPercentage = updatedDiscount.DiscountPercentage;
            discount.StartDate = updatedDiscount.StartDate;
            discount.EndDate = updatedDiscount.EndDate;

            await _context.SaveChangesAsync();
            return discount;
        }

        public async Task<bool> DeleteDiscountAsync(int id)
        {
            var discount = await _context.Discounts.FindAsync(id);
            if (discount == null)
            {
                return false;
            }

            _context.Discounts.Remove(discount);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IsDiscountExistAsync(int id)
        {
            return await _context.Discounts.AnyAsync(d => d.Id == id);
        }

        public async Task<bool> AddProductToDiscountAsync(int discountId, int productId)
        {
            var discount = await _context.Discounts.FindAsync(discountId);
            if (discount == null)
            {
                return false;
            }

            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                return false;
            }

            var exists = await _context.ProductDiscounts.AnyAsync(
                pd => pd.DiscountId == discountId && pd.ProductId == productId);

            if (exists)
            {
                return false;
            }

            var productDiscount = new ProductDiscount
            {
                DiscountId = discountId,
                ProductId = productId
            };

            _context.ProductDiscounts.Add(productDiscount);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveProductFromDiscountAsync(int discountId, int productId)
        {
            var productDiscount = await _context.ProductDiscounts
                .FirstOrDefaultAsync(pd => pd.DiscountId == discountId && pd.ProductId == productId);

            if (productDiscount == null)
            {
                return false;
            }

            _context.ProductDiscounts.Remove(productDiscount);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Product>> GetProductsByDiscountIdAsync(int discountId)
        {
            // return await _context.ProductDiscounts
            //     .Where(pd => pd.DiscountId == discountId)
            //     .Select(pd => pd.Product!)
            //     .Include(p => p.Colors)
            //         .ThenInclude(c => c.Images)
            //     .ToListAsync();
            return await _context.ProductDiscounts
                .Where(pd => pd.DiscountId == discountId)
                .Include(pd => pd.Product!)
                .ThenInclude(p => p.Colors)
                    .ThenInclude(c => c.Images)
                .Select(pd => pd.Product!)
                .ToListAsync();
        }


        public async Task<IEnumerable<Discount>> GetDiscountsByProductIdAsync(int productId)
        {
            return await _context.ProductDiscounts
                .Where(pd => pd.ProductId == productId)
                .Select(pd => pd.Discount!)
                .ToListAsync();
        }
    }
}
