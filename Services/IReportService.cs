using Microsoft.EntityFrameworkCore;
using MobileShop.Data;
using MobileShop.Models;
using MobileShop.ViewModels;

namespace MobileShop.Services
{
    public interface IReportService
    {
        Task<DashboardViewModel> GetDashboardDataAsync();
        Task<SalesReportViewModel> GetSalesReportAsync(DateTime startDate, DateTime endDate);
        Task<InventoryReportViewModel> GetInventoryReportAsync();
        Task<List<TopProductViewModel>> GetTopSellingProductsAsync(int count = 10);
    }
    public class ReportService : IReportService
    {
        private readonly ApplicationDbContext _context;

        public ReportService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardViewModel> GetDashboardDataAsync()
        {
            var now = DateTime.Now;
            var thirtyDaysAgo = now.AddDays(-30);

            var totalOrders = await _context.Orders.CountAsync();
            var totalProducts = await _context.Products.CountAsync();
            var totalCustomers = await _context.Users.CountAsync();
            var totalRevenue = await _context.Orders
                .Where(o => o.PaymentStatus == PaymentStatus.Paid)
                .SumAsync(o => o.TotalAmount);
            var pendingOrders = await _context.Orders
                .CountAsync(o => o.Status == OrderStatus.Pending);
            var lowStockProducts = await _context.Products
                .CountAsync(p => p.StockQuantity <= 10);

            var recentOrders = await _context.Orders
                .Include(o => o.User)
                .OrderByDescending(o => o.OrderDate)
                .Take(10)
                .Select(o => new RecentOrderViewModel
                {
                    OrderId = o.Id,
                    OrderNumber = o.OrderNumber,
                    CustomerName = o.User != null ? o.User.FullName : "Guest",
                    TotalAmount = o.TotalAmount,
                    Status = o.Status.ToString(),
                    OrderDate = o.OrderDate
                })
                .ToListAsync();

            var topProducts = _context.OrderItems
                .Include(oi => oi.Product)
                .AsEnumerable()
                .GroupBy(oi => new { oi.ProductId, oi.Product.Name })
                .Select(g => new TopProductViewModel
                {
                    ProductId = g.Key.ProductId,
                    ProductName = g.Key.Name,
                    UnitsSold = g.Sum(oi => oi.Quantity),
                    Revenue = g.Sum(oi => oi.TotalPrice)
                })
                .OrderByDescending(tp => tp.Revenue)
                .Take(5)
                .ToList();

            var monthlySales = new List<MonthlySalesViewModel>();
            for (int i = 5; i >= 0; i--)
            {
                var month = now.AddMonths(-i);
                var monthStart = new DateTime(month.Year, month.Month, 1);
                var monthEnd = monthStart.AddMonths(1);

                var sales = await _context.Orders
                    .Where(o => o.OrderDate >= monthStart && o.OrderDate < monthEnd && o.PaymentStatus == PaymentStatus.Paid)
                    .SumAsync(o => o.TotalAmount);

                var orders = await _context.Orders
                    .CountAsync(o => o.OrderDate >= monthStart && o.OrderDate < monthEnd);

                monthlySales.Add(new MonthlySalesViewModel
                {
                    Month = month.ToString("MMM yyyy"),
                    Sales = sales,
                    Orders = orders
                });
            }

            return new DashboardViewModel
            {
                TotalOrders = totalOrders,
                TotalProducts = totalProducts,
                TotalCustomers = totalCustomers,
                TotalRevenue = totalRevenue,
                PendingOrders = pendingOrders,
                LowStockProducts = lowStockProducts,
                RecentOrders = recentOrders,
                TopProducts = topProducts,
                MonthlySales = monthlySales
            };
        }

        public async Task<InventoryReportViewModel> GetInventoryReportAsync()
        {
            var totalProducts = await _context.Products.CountAsync();
            var lowStockProducts = await _context.Products
                .Where(p => p.StockQuantity <= 10 && p.StockQuantity > 0)
                .Select(p => new LowStockProductViewModel
                {
                    ProductId = p.Id,
                    ProductName = p.Name,
                    CurrentStock = p.StockQuantity
                })
                .ToListAsync();

            var outOfStockCount = await _context.Products
                .CountAsync(p => p.StockQuantity == 0);

            return new InventoryReportViewModel
            {
                TotalProducts = totalProducts,
                LowStockCount = lowStockProducts.Count,
                OutOfStockCount = outOfStockCount,
                LowStockProducts = lowStockProducts
            };
        }

        public async Task<List<TopProductViewModel>> GetTopSellingProductsAsync(int count = 10)
        {
            return _context.OrderItems
                .Include(oi => oi.Product)
                .GroupBy(oi => new { oi.ProductId, oi.Product.Name })
                .AsEnumerable()
                .Select(g => new TopProductViewModel
                {
                    ProductId = g.Key.ProductId,
                    ProductName = g.Key.Name,
                    UnitsSold = g.Sum(oi => oi.Quantity),
                    Revenue = g.Sum(oi => oi.TotalPrice)
                })
                .OrderByDescending(tp => tp.UnitsSold)
                .Take(count)
                .ToList();
        }
    
        public async Task<SalesReportViewModel> GetSalesReportAsync(DateTime startDate, DateTime endDate)
        {
            var orders = await _context.Orders
                .Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate)
                .ToListAsync();

            var totalSales = orders
                .Where(o => o.PaymentStatus == PaymentStatus.Paid)
                .Sum(o => o.TotalAmount);

            var dailySales = orders
                .GroupBy(o => o.OrderDate.Date)
                .Select(g => new DailySalesViewModel
                {
                    Date = g.Key,
                    Sales = g.Where(o => o.PaymentStatus == PaymentStatus.Paid).Sum(o => o.TotalAmount),
                    OrderCount = g.Count()
                })
                .OrderBy(ds => ds.Date)
                .ToList();

            return new SalesReportViewModel
            {
                StartDate = startDate,
                EndDate = endDate,
                TotalSales = totalSales,
                TotalOrders = orders.Count,
                AverageOrderValue = orders.Any() ? totalSales / orders.Count : 0,
                DailySales = dailySales
            };
        }

    }
}
