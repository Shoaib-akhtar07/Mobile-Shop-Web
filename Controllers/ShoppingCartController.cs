using Microsoft.AspNetCore.Mvc;
using MobileShop.Services;

namespace MobileShop.Controllers
{
    public class ShoppingCartController : Controller
    {
        private readonly IShoppingCartService _cartService;

        public ShoppingCartController(IShoppingCartService cartService)
        {
            _cartService = cartService;
        }
        public async Task<IActionResult> Index()
        {
            var cart = await _cartService.GetCartAsync();
            ViewBag.CartItemCount = cart.ItemCount;
            return View(cart);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            await _cartService.AddToCartAsync(productId, quantity);
            var itemCount = await _cartService.GetCartItemCountAsync();

            if (Request.Headers.XRequestedWith == "XMLHttpRequest")
            {
                return Json(new { success = true, itemCount, message = "Product added to cart!" });
            }

            TempData["Success"] = "Product added to cart!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int cartItemId, int quantity)
        {
            await _cartService.UpdateCartItemAsync(cartItemId, quantity);
            var cart = await _cartService.GetCartAsync();
            if (Request.Headers.XRequestedWith == "XMLHttpRequest")
            {
                return Json(new
                {
                    success = true,
                    itemCount = cart.ItemCount,
                    cartTotal = cart.CartTotal,
                    grandTotal = cart.GrandTotal,
                    tax = cart.Tax,
                    shipping = cart.Shipping
                });
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> RemoveItem(int cartItemId)
        {
            await _cartService.RemoveFromCartAsync(cartItemId);
            var cart = await _cartService.GetCartAsync();

            if (Request.Headers.XRequestedWith == "XMLHttpRequest")
            {
                return Json(new
                {
                    success = true,
                    itemCount = cart.ItemCount,
                    cartTotal = cart.CartTotal,
                    grandTotal = cart.GrandTotal
                });
            }

            TempData["Success"] = "Item removed from cart.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> ClearCart()
        {
            await _cartService.ClearCartAsync();
            if (Request.Headers.XRequestedWith == "XMLHttpRequest")
            {
                return Json(new { success = true, itemCount = 0 });
            }

            TempData["Success"] = "Cart cleared.";
            return RedirectToAction(nameof(Index));
        }

    }
}