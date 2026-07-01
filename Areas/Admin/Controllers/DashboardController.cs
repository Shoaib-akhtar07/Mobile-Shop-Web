using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MobileShop.Data;
using MobileShop.Models;
using MobileShop.Services;


namespace MobileShop.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly IReportService _reportService;

        public DashboardController(
            IReportService reportService
           )
        {
            _reportService = reportService;
           
        }

        public async Task<IActionResult> Index()
        {
            var dashboardData = await _reportService.GetDashboardDataAsync();
            return View(dashboardData);
        }
        public async Task<IActionResult> SalesReport(DateTime? startDate, DateTime? endDate)
        {
            var start = startDate ?? DateTime.Now.AddDays(-30);
            var end = endDate ?? DateTime.Now;

            var report = await _reportService.GetSalesReportAsync(start, end);
            return View(report);
        }
        public async Task<IActionResult> InventoryReport()
        {
            var report = await _reportService.GetInventoryReportAsync();
            return View(report);
        }
        public async Task<IActionResult> TopProducts()
        {
            var products = await _reportService.GetTopSellingProductsAsync(20);
            return View(products);
        }
    }
}