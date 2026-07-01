using Microsoft.EntityFrameworkCore;
using MobileShop.Data;
using MobileShop.Models;
using MobileShop.ViewModels;

namespace MobileShop.Services
{
    public interface IShoppingCartService
    {
        Task AddToCartAsync(int productId, int quantity = 1);
        Task<int> GetCartItemCountAsync();
        Task<ShoppingCartViewModel> GetCartAsync();
        Task UpdateCartItemAsync(int cartItemId, int quantity);
        Task RemoveFromCartAsync(int cartItemId);
        Task ClearCartAsync();         
        Task MigrateCartAsync(string userId);

    }

    public class ShoppingCartService : IShoppingCartService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ApplicationDbContext _context;
        public ShoppingCartService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }
        private string GetCartId()
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            var cartId = session?.GetString("CartId");

            if (string.IsNullOrEmpty(cartId))
            {
                cartId = Guid.NewGuid().ToString();
                session?.SetString("CartId", cartId);
            }

            return cartId;
        }
        public async Task AddToCartAsync(int productId, int quantity = 1)
        {
            var cartId = GetCartId();
            var cartItem = await _context.ShoppingCartItems
               .FirstOrDefaultAsync(c => c.CartId == cartId && c.ProductId == productId);
            if (cartItem != null)
            {
                cartItem.Quantity += quantity;
            }
            else
            {
                cartItem = new ShoppingCartItem
                {
                    CartId = cartId,
                    ProductId = productId,
                    Quantity = quantity
                };
                _context.ShoppingCartItems.Add(cartItem);
            }
            await _context.SaveChangesAsync();
        }

        public async Task<int> GetCartItemCountAsync()
        {
            var cartId = GetCartId();
            return await _context.ShoppingCartItems
                        .Where(c => c.CartId == cartId)
                        .SumAsync(c => c.Quantity);

        }

        public async Task<ShoppingCartViewModel> GetCartAsync()
        {
            var cartId = GetCartId();
            var cartItems = await _context.ShoppingCartItems
                .Include(c => c.Product)
                .Where(c => c.CartId == cartId)
                .ToListAsync();

            var viewModel = new ShoppingCartViewModel
            {
                CartItems = cartItems.Select(c => new CartItemViewModel
                {
                    CartItemId = c.Id,
                    ProductId = c.ProductId,
                    ProductName = c.Product.Name,
                    ProductImage = c.Product.MainImageUrl,
                    UnitPrice = c.Product.SalePrice,
                    Quantity = c.Quantity,
                    StockQuantity = c.Product.StockQuantity
                }).ToList(),
                CartTotal = cartItems.Sum(c => c.Product.SalePrice * c.Quantity),
                ItemCount = cartItems.Sum(c => c.Quantity),
                Tax = cartItems.Sum(c => c.Product.SalePrice * c.Quantity) * 0.18m,
                Shipping = cartItems.Sum(c => c.Product.SalePrice * c.Quantity) > 50000 ? 0 : 500
            };

            return viewModel;

        }

        public async Task UpdateCartItemAsync(int cartItemId, int quantity)
        {
            var cartId = GetCartId();
            var cartItem = await _context.ShoppingCartItems
                .FirstOrDefaultAsync(c => c.Id == cartItemId && c.CartId == cartId);
            if (cartItem != null)
            {
                if (quantity <= 0)
                {
                    _context.ShoppingCartItems.Remove(cartItem);
                }
                else
                {
                    cartItem.Quantity = quantity;
                }

                await _context.SaveChangesAsync();
            }
        }

        public async Task RemoveFromCartAsync(int cartItemId)
        {
            var cartId = GetCartId();
            var cartItem = await _context.ShoppingCartItems
                .FirstOrDefaultAsync(c => c.Id == cartItemId && c.CartId == cartId);

            if (cartItem != null)
            {
                _context.ShoppingCartItems.Remove(cartItem);
                await _context.SaveChangesAsync();
            }
        }

        public async Task ClearCartAsync()
        {
            var cartId = GetCartId();
            var cartItems = await _context.ShoppingCartItems
                .Where(c => c.CartId == cartId)
                .ToListAsync();

            _context.ShoppingCartItems.RemoveRange(cartItems);
            await _context.SaveChangesAsync();
        }

        public async Task MigrateCartAsync(string userId)
        {
            var cartId = GetCartId();
            var cartItems = await _context.ShoppingCartItems
                .Where(c => c.CartId == cartId)
                .ToListAsync();

            foreach (var item in cartItems)
            {
                item.CartId = userId;
            }

            await _context.SaveChangesAsync();
            _httpContextAccessor.HttpContext?.Session.Remove("CartId");
        }
    }
}