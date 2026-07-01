using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MobileShop.Models;
using MobileShop.Services;
using MobileShop.ViewModels;
using Stripe;


namespace MobileShop.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly IShoppingCartService _cartService;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService ;
        private readonly IOrderService  _orderService;
        private readonly UserManager<ApplicationUser> _userManager;

        public CheckoutController(
            IShoppingCartService cartService,
            IConfiguration configuration,
            IEmailService emailService,
            IOrderService orderService,
            UserManager<ApplicationUser> userManager)
        {
            _cartService = cartService;
            _configuration = configuration;
            _userManager = userManager;
            _emailService = emailService;
            _orderService = orderService;

        }
        public async Task<IActionResult> Index()
        {
            var cart = await _cartService.GetCartAsync();
            if (cart.CartItems.Count == 0)
            {
                TempData["Error"] = "Your cart is empty.";
                return RedirectToAction("Index", "ShoppingCart");
            }

            var user = await _userManager.GetUserAsync(User);
            var model = new CheckoutViewModel
            {
                Cart = cart,
                IsAuthenticated = user != null,
                FirstName = user?.FirstName ?? "",
                LastName = user?.LastName ?? "",
                Email = user?.Email ?? "",
                Phone = user?.PhoneNumber ?? "",
                Address = user?.Address ?? "",
                City = user?.City ?? "",
                PostalCode = user?.PostalCode ?? "",
                Country = user?.Country ?? "Pakistan"
            };
            ViewBag.CartItemCount = cart.ItemCount;
            ViewBag.StripePublishableKey = _configuration["Stripe:PublishableKey"];
            return View(model);
        }
        private async Task<IActionResult> ProcessStripePayment(Order order, CheckoutViewModel model)
        {
            try
            {
                var options = new ChargeCreateOptions
                {
                    Amount = (long)(order.TotalAmount * 100),
                    Currency = "PKR",
                    Description = "Order {order.OrderNumber}",
                    Source = model.StripeToken
                };

                var service = new ChargeService();
                var charge = await service.CreateAsync(options);

                if (charge.Status == "succeeded")
                {
                    await _orderService.ProcessPaymentAsync(order.Id, charge.Id);
                    await _cartService.ClearCartAsync();
                    await _emailService.SendOrderConfirmationAsync(model.Email, order.OrderNumber, order.TotalAmount);

                    return RedirectToAction("Confirmation", new { orderNumber = order.OrderNumber });
                }
                else
                {
                    TempData["Error"] = "Payment failed. Please try again.";
                    return RedirectToAction("Index");
                }
            }
            catch (StripeException ex)
            {
                
                TempData["Error"] = $"Payment error: {ex.Message}";
                return RedirectToAction("Index");
            }
        }
        public async Task<IActionResult> Confirmation(string orderNumber)
        {
            var order = await _orderService.GetOrderByNumberAsync(orderNumber);
            if (order == null)
            {
                return NotFound();
            }

            var viewModel = new OrderConfirmationViewModel
            {
                Order = order,
                OrderItems = order.OrderItems.ToList()
            };

            ViewBag.CartItemCount = 0;
            return View(viewModel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Process(CheckoutViewModel model)
        {
            var cart = await _cartService.GetCartAsync();
            if (cart.CartItems.Count == 0)
            {
                TempData["Error"] = "Your cart is empty.";
                return RedirectToAction("Index", "ShoppingCart");
            }

            model.Cart = cart;

            if (!ModelState.IsValid)
            {
                ViewBag.CartItemCount = cart.ItemCount;
                ViewBag.StripePublishableKey = _configuration["Stripe:PublishableKey"];
                return View("Index", model);
            }

            try
            {
                var user = await _userManager.GetUserAsync(User);
                var userId = user?.Id;

                // Create order
                var order = await _orderService.CreateOrderAsync(model, userId);

                // Process payment based on method
                if (model.PaymentMethod == MobileShop.Models.PaymentMethod.Stripe)
                {
                    return await ProcessStripePayment(order, model);
                }
                else if (model.PaymentMethod == MobileShop.Models.PaymentMethod.CashOnDelivery)
                {
                    order.PaymentStatus = PaymentStatus.Pending;
                    await _orderService.UpdateOrderStatusAsync(order.Id, OrderStatus.Pending);
                }
                else
                {
                    // For other payment methods, mark as paid for demo
                    await _orderService.ProcessPaymentAsync(order.Id, $"DEMO-{Guid.NewGuid()}");
                }

                // Clear cart
                await _cartService.ClearCartAsync();

                // Send confirmation email
                await _emailService.SendOrderConfirmationAsync(model.Email, order.OrderNumber, order.TotalAmount);

                return RedirectToAction("Confirmation", new { orderNumber = order.OrderNumber });
            }
            catch (Exception ex)
            {
                
                ModelState.AddModelError("", "An error occurred during checkout. Please try again.");
                ViewBag.CartItemCount = cart.ItemCount;
                return View("Index", model);
            }
        }
        public async Task<IActionResult> TrackOrder(string? orderNumber)
        {
            if (string.IsNullOrWhiteSpace(orderNumber))
            {
                return View();
            }

            var order = await _orderService.GetOrderByNumberAsync(orderNumber);
            if (order == null)
            {
                TempData["Error"] = "Order not found.";
                return View();
            }

            return View(order);
        }
    }
}
