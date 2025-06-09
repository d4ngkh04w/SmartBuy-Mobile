using api.Database;
using api.DTOs.Dashboard;
using api.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace api.Repositories
{
    public class DashboardRepository : IDashboardRepository
    {
        private readonly AppDBContext _db;

        public DashboardRepository(AppDBContext db)
        {
            _db = db;
        }

        public async Task<List<TopProductDTO>> GetTopProductsAsync(DateTime? startDate, DateTime? endDate, string sortBy)
        {
            var query = _db.Orders
                .Where(o => o.Status == "Hoàn thành");

            if (startDate.HasValue)
            {
                query = query.Where(o => o.DeliveryDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(o => o.DeliveryDate <= endDate.Value);
            }

            var result = await query
                .SelectMany(o => o.OrderItems)
                .GroupBy(oi => new { oi.ProductId, oi.Product!.Name })
                .Select(g => new TopProductDTO
                {
                    ProductId = g.Key.ProductId,
                    ProductName = g.Key.Name,
                    Quantity = g.Sum(oi => oi.Quantity),
                    Revenue = g.Sum(oi => oi.Price * oi.Quantity * (1 - oi.Discount / 100)),
                    CreatedAt = g.First().Product!.CreatedAt
                })
                .AsNoTracking()
                .ToListAsync();
            foreach (var product in result)
            {
                product.CreatedAtFormatted = product.CreatedAt.ToString("yyyy-MM-dd");
            }

            return sortBy.Equals("revenue", StringComparison.OrdinalIgnoreCase)
                ? result.OrderByDescending(p => p.Revenue).ToList()
                : result.OrderByDescending(p => p.Quantity).ToList();
        }

        public async Task<OrderStatisticsDTO> GetOrderStatisticsAsync(DateTime? startDate, DateTime? endDate)
        {
            var currentQuery = _db.Orders.AsQueryable();
            var previousQuery = _db.Orders.AsQueryable();

            if (startDate.HasValue)
            {
                currentQuery = currentQuery.Where(o => o.OrderDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                currentQuery = currentQuery.Where(o => o.OrderDate <= endDate.Value);
            }

            if (startDate.HasValue && endDate.HasValue)
            {
                var periodLength = endDate.Value - startDate.Value;
                var previousStart = startDate.Value - periodLength;
                var previousEnd = startDate.Value;

                previousQuery = previousQuery.Where(o => o.OrderDate >= previousStart && o.OrderDate < previousEnd);
            }

            var currentOrders = await currentQuery.AsNoTracking().ToListAsync();
            var currentCompleted = currentOrders.Where(o => o.Status == "Hoàn thành").ToList();

            var totalOrders = currentOrders.Count;
            var totalRevenue = currentCompleted.Sum(o => o.TotalAmount);
            var avgOrderValue = totalOrders > 0 ? totalRevenue / totalOrders : 0;
            var completionRate = totalOrders > 0 ? (decimal)currentCompleted.Count / totalOrders * 100 : 0;

            var previousOrders = await previousQuery.AsNoTracking().ToListAsync();
            var previousCompleted = previousOrders.Where(o => o.Status == "Hoàn thành").ToList();

            var prevTotalOrders = previousOrders.Count;
            var prevTotalRevenue = previousCompleted.Sum(o => o.TotalAmount);
            var prevAvgOrderValue = prevTotalOrders > 0 ? prevTotalRevenue / prevTotalOrders : 0;
            var prevCompletionRate = prevTotalOrders > 0 ? (decimal)previousCompleted.Count / prevTotalOrders * 100 : 0;

            // Calculate percentage changes
            var orderChange = prevTotalOrders > 0 ? (decimal)(totalOrders - prevTotalOrders) / prevTotalOrders * 100 : 0;
            var revenueChange = prevTotalRevenue > 0 ? (totalRevenue - prevTotalRevenue) / prevTotalRevenue * 100 : 0;
            var avgChange = prevAvgOrderValue > 0 ? (avgOrderValue - prevAvgOrderValue) / prevAvgOrderValue * 100 : 0;
            var completionChange = prevCompletionRate > 0 ? (completionRate - prevCompletionRate) / prevCompletionRate * 100 : 0;

            return new OrderStatisticsDTO
            {
                TotalOrders = totalOrders,
                TotalRevenue = totalRevenue,
                AvgOrderValue = avgOrderValue,
                CompletionRate = completionRate,
                OrderChange = orderChange,
                RevenueChange = revenueChange,
                AvgChange = avgChange,
                CompletionChange = completionChange
            };
        }

        public async Task<List<OrderStatusDistributionDTO>> GetOrderStatusDistributionAsync(DateTime? startDate, DateTime? endDate)
        {
            var query = _db.Orders.AsQueryable();

            if (startDate.HasValue)
            {
                query = query.Where(o => o.OrderDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(o => o.OrderDate <= endDate.Value);
            }

            var statusCounts = await query
                .GroupBy(o => o.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .AsNoTracking()
                .ToListAsync();

            var totalOrders = statusCounts.Sum(s => s.Count);

            return statusCounts.Select(s => new OrderStatusDistributionDTO
            {
                Status = s.Status,
                Count = s.Count,
                Percentage = totalOrders > 0 ? Math.Round((decimal)s.Count / totalOrders * 100, 1) : 0
            }).OrderByDescending(s => s.Count).ToList();
        }

        public async Task<List<PaymentMethodStatDTO>> GetPaymentMethodStatsAsync(DateTime? startDate, DateTime? endDate)
        {
            var query = _db.Orders.AsQueryable();

            if (startDate.HasValue)
            {
                query = query.Where(o => o.OrderDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(o => o.OrderDate <= endDate.Value);
            }

            var paymentCounts = await query
                .GroupBy(o => o.PaymentMethod)
                .Select(g => new { Method = g.Key, Count = g.Count() })
                .AsNoTracking()
                .ToListAsync();

            var totalOrders = paymentCounts.Sum(p => p.Count);

            return paymentCounts.Select(p => new PaymentMethodStatDTO
            {
                Method = p.Method,
                Count = p.Count,
                Percentage = totalOrders > 0 ? Math.Round((decimal)p.Count / totalOrders * 100, 1) : 0
            }).OrderByDescending(p => p.Count).ToList();
        }

        public async Task<List<RevenueChartDTO>> GetRevenueChartDataAsync(DateTime? startDate, DateTime? endDate, string period)
        {
            var completedOrders = _db.Orders
                .Where(o => o.Status == "Hoàn thành");

            if (startDate.HasValue)
            {
                completedOrders = completedOrders.Where(o => o.DeliveryDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                completedOrders = completedOrders.Where(o => o.DeliveryDate <= endDate.Value);
            }

            completedOrders = completedOrders.Where(o => o.DeliveryDate.HasValue);

            switch (period.ToLower())
            {
                case "day":
                    var dailyResults = await completedOrders
                        .GroupBy(o => o.DeliveryDate!.Value.Date)
                        .Select(g => new
                        {
                            Date = g.Key,
                            Amount = g.Sum(o => o.TotalAmount),
                            OrderCount = g.Count()
                        })
                        .OrderBy(r => r.Date)
                        .AsNoTracking()
                        .ToListAsync();

                    return dailyResults.Select(r => new RevenueChartDTO
                    {
                        Date = r.Date,
                        DisplayDate = r.Date.ToString("yyyy-MM-dd"),
                        Amount = r.Amount,
                        OrderCount = r.OrderCount
                    }).ToList();

                case "month":
                default:
                    var monthlyResults = await completedOrders
                        .GroupBy(o => new { o.DeliveryDate!.Value.Year, o.DeliveryDate!.Value.Month })
                        .Select(g => new
                        {
                            g.Key.Year,
                            g.Key.Month,
                            Amount = g.Sum(o => o.TotalAmount),
                            OrderCount = g.Count()
                        })
                        .OrderBy(r => r.Year)
                        .ThenBy(r => r.Month)
                        .AsNoTracking()
                        .ToListAsync();

                    return monthlyResults.Select(r => new RevenueChartDTO
                    {
                        Date = new DateTime(r.Year, r.Month, 1),
                        DisplayDate = $"{r.Year}-{r.Month:D2}",
                        Amount = r.Amount,
                        OrderCount = r.OrderCount
                    }).ToList();
            }
        }
    }
}
