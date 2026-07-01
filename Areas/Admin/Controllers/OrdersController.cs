using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MobileShop.Data;
using MobileShop.Models;
using MobileShop.Services;

namespace MobileShop.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IOrderService _orderService;
        private readonly IEmailService _emailService;

        public OrdersController(ApplicationDbContext context, IOrderService orderService , IEmailService emailService   )
        {
            _context = context;
            _orderService = orderService;
            _emailService = emailService;
        }

        public async Task<IActionResult> Index(string? status, string? search, int page = 1)
        {
            var query = _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<OrderStatus>(status, out var orderStatus))
                query = query.Where(o => o.Status == orderStatus);

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(o => o.OrderNumber.Contains(search) ||
                                        (o.User != null && (o.User.Email.Contains(search) || o.User.FirstName.Contains(search) || o.User.LastName.Contains(search))));

            var pageSize = 20;
            var totalItems = await query.CountAsync();
            var orders = await query
                .OrderByDescending(o => o.OrderDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Statuses = Enum.GetNames(typeof(OrderStatus));
            ViewBag.CurrentStatus = status;
            ViewBag.Search = search;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            return View(orders);
        }
        public async Task<IActionResult> Invoice(int id)
        {
            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                return NotFound();

            return View(order);
        }
        public async Task<IActionResult> Details(int id)
        {
            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                return NotFound();

            ViewBag.Statuses = Enum.GetValues(typeof(OrderStatus)).Cast<OrderStatus>().ToList();
            return View(order);
        }
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, OrderStatus status)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
                return NotFound();

            var result = await _orderService.UpdateOrderStatusAsync(id, status);

            if (result)
            {
                // FIX: Update payment status to Paid for COD/UPI/Cash orders when delivered
                if (status == OrderStatus.Delivered &&
                    (order.PaymentMethod == PaymentMethod.CashOnDelivery ||
                     order.PaymentMethod == PaymentMethod.UPI ||
                     order.PaymentMethod == PaymentMethod.CreditCard ||
                     order.PaymentMethod == PaymentMethod.DebitCard))
                {
                    if (order.PaymentStatus != PaymentStatus.Paid)
                    {
                        //await _orderService.ProcessPaymentAsync(order.Id, $"DEMO-{Guid.NewGuid()}");
                        order.PaymentStatus = PaymentStatus.Paid;
                        await _context.SaveChangesAsync();
                    }
                }

                // Send email notification
                if (order.UserId != null)
                {
                    var user = await _context.Users.FindAsync(order.UserId);
                    if (user != null)
                    {
                        await _emailService.SendOrderStatusUpdateAsync(user.Email!, order.OrderNumber, status.ToString());
                    }
                }

                TempData["Success"] = $"Order status updated to {status}.";

                // Add payment status message if updated
                if (status == OrderStatus.Delivered && order.PaymentStatus == PaymentStatus.Paid)
                {
                    TempData["Success"] += " Payment marked as Paid.";
                }
            }
            else
            {
                TempData["Error"] = "Failed to update order status.";
            }

            return RedirectToAction(nameof(Details), new { id });
        }
        [HttpPost]
        public async Task<IActionResult> CancelOrder(int id)
        {
            var result = await _orderService.UpdateOrderStatusAsync(id, OrderStatus.Cancelled);
            if (result)
            {
                // Restore stock
                var order = await _context.Orders
                    .Include(o => o.OrderItems)
                    .FirstOrDefaultAsync(o => o.Id == id);

                if (order != null)
                {
                    foreach (var item in order.OrderItems)
                    {
                        var product = await _context.Products.FindAsync(item.ProductId);
                        if (product != null)
                        {
                            product.StockQuantity += item.Quantity;
                        }
                    }
                    await _context.SaveChangesAsync();
                }

                TempData["Success"] = "Order cancelled successfully.";
            }
            else
            {
                TempData["Error"] = "Failed to cancel order.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
