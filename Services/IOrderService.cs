using Microsoft.EntityFrameworkCore;
using MobileShop.Data;
using MobileShop.Models;
using MobileShop.ViewModels;

namespace MobileShop.Services
{
    public interface IOrderService
    {
        Task<Order> CreateOrderAsync(CheckoutViewModel model, string? userId);
        Task<Order?> GetOrderByNumberAsync(string orderNumber);
        Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatus status);
        Task<bool> ProcessPaymentAsync(int orderId, string transactionId);
        Task<List<Order>> GetUserOrdersAsync(string userId);
        Task<Order?> GetOrderByIdAsync(int orderId);

    }

    public class OrderService : IOrderService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<OrderService> _logger;

        public OrderService(ApplicationDbContext context, ILogger<OrderService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Order> CreateOrderAsync(CheckoutViewModel model, string? userId)
        {
            var order = new Order
            {
                UserId = userId,
                Subtotal = model.Cart.CartTotal,
                TaxAmount = model.Cart.Tax,
                ShippingCost = model.Cart.Shipping,
                TotalAmount = model.Cart.GrandTotal,
                PaymentMethod = model.PaymentMethod,
                ShippingAddress = model.Address,
                ShippingCity = model.City,
                ShippingPostalCode = model.PostalCode,
                ShippingCountry = model.Country,
                ShippingPhone = model.Phone,
                Notes = model.OrderNotes
            };

            foreach (var item in model.Cart.CartItems)
            {
                order.OrderItems.Add(new OrderItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice
                });

                // Update stock
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product != null)
                {
                    product.StockQuantity -= item.Quantity;
                }
            }

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Order {order.OrderNumber} created successfully");
            return order;
        }

        public async Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatus status)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null) return false;

            order.Status = status;

            if (status == OrderStatus.Shipped)
            {
                order.ShippedDate = DateTime.Now;
            }
            else if (status == OrderStatus.Delivered)
            {
                order.DeliveredDate = DateTime.Now;

                // FIX: Mark payment as Paid for COD and manual payment methods on delivery
                if (order.PaymentStatus == PaymentStatus.Pending &&
                    (order.PaymentMethod == PaymentMethod.CashOnDelivery ||
                     order.PaymentMethod == PaymentMethod.UPI ||
                     order.PaymentMethod == PaymentMethod.CreditCard ||
                     order.PaymentMethod == PaymentMethod.DebitCard))
                {
                    order.PaymentStatus = PaymentStatus.Paid;
                }
            }
            else if (status == OrderStatus.Refunded)
            {
                // FIX: When order is refunded, payment status must also be Refunded
                order.PaymentStatus = PaymentStatus.Refunded;
            }
            else if (status == OrderStatus.Cancelled)
            {
                // FIX: If order was already paid and then cancelled, mark payment as Refunded
                if (order.PaymentStatus == PaymentStatus.Paid)
                {
                    order.PaymentStatus = PaymentStatus.Refunded;
                }
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation($"Order {orderId} status updated to {status}");
            return true;
        }

        public async Task<bool> ProcessPaymentAsync(int orderId, string transactionId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null) return false;

            order.PaymentStatus = PaymentStatus.Paid;
            order.TransactionId = transactionId;

            await _context.SaveChangesAsync();
            _logger.LogInformation($"Payment processed for order {orderId}");
            return true;
        }

        public async Task<Order?> GetOrderByNumberAsync(string orderNumber)
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber);
        }

        public async Task<List<Order>> GetUserOrdersAsync(string userId)
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }
        public async Task<Order?> GetOrderByIdAsync(int orderId)
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == orderId);
        }
    }
}