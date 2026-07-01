using DocumentFormat.OpenXml.InkML;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MobileShop.Models;
using MobileShop.Services;
using MobileShop.ViewModels;
using System.Threading.Tasks;

namespace MobileShop.Controllers
{
    public class ProductsController : Controller
    {
        private readonly IProductService _productService;
        private readonly IShoppingCartService _cartService;
        private readonly UserManager<ApplicationUser> _userManager;
       
       


        // Injecting the Product Service through the constructor
        public ProductsController(IProductService productService, IShoppingCartService cartService,  UserManager<ApplicationUser> userManager )
        {
            _productService = productService;
            _cartService = cartService;
            _userManager = userManager;
            
        }

        /// <summary>
        /// Displays a filtered and paginated list of products
        /// </summary>
        public async Task<IActionResult> Index(
            int? categoryId,
            int? brandId,
            string? search,
            string? sort,
            decimal? minPrice,
            decimal? maxPrice,
            int page = 1)
        {
            // Mapping URL parameters to our ViewModel
            var filter = new ProductListViewModel
            {
                SelectedCategoryId = categoryId,
                SelectedBrandId = brandId,
                SearchTerm = search,
                SortOrder = sort,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                PageNumber = page
            };

            // Calling the service to handle the filtering logic
            var result = await _productService.GetProductsAsync(filter);
            ViewBag.CartItemCount = await _cartService.GetCartItemCountAsync();

            return View(result);
        }
        [HttpGet]
        public async Task<IActionResult> Search(string term)
        {
            if (string.IsNullOrWhiteSpace(term) || term.Length < 2)
                return Json(new List<object>());

            var filter = new ProductListViewModel
            {
                SearchTerm = term,
                PageSize = 5
            };

            var result = await _productService.GetProductsAsync(filter);
            var suggestions = result.Products.Select(p => new
            {
                id = p.Id,
                name = p.Name,
                price = p.SalePrice,
                image = p.MainImageUrl,
                brand = p.Brand?.Name
            });

            return Json(suggestions);
        }
        /// <summary>
        /// Displays the detailed information for a specific product
        /// </summary>
        public async Task<IActionResult> Details(int id)
        {
            // 1. Fetch the main product details from the service
            var product = await _productService.GetProductByIdAsync(id);

            // 2. Safety Check: If product doesn't exist or is inactive, return 404
            if (product == null)
            {
                return NotFound();
            }

            // 3. Fetch related products (upselling)
            var relatedProducts = await _productService.GetRelatedProductsAsync(id);

            // 4. Placeholder for Wishlist logic (can be expanded later)
            bool isInWishlist = false;
            if (User.Identity?.IsAuthenticated == true)
            {
                var user = await _userManager.GetUserAsync(User);
                isInWishlist = user?.WishlistItems.Any(w => w.ProductId == id) ?? false;
            }

            // 5. Build the ViewModel
            var viewModel = new ProductDetailViewModel
            {
                Product = product,
                RelatedProducts = relatedProducts,
                IsInWishlist = isInWishlist
            };
            ViewBag.CartItemCount = await _cartService.GetCartItemCountAsync();

            return View(viewModel);
        }
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddReview(int productId, ReviewViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please fill in all required fields.";
                return RedirectToAction(nameof(Details), new { id = productId });
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var result = await _productService.AddReviewAsync(productId, user.Id, model);
            if (result)
            {
                TempData["Success"] = "Thank you for your review!";
            }
            else
            {
                TempData["Error"] = "Failed to add review. Please try again.";
            }

            return RedirectToAction(nameof(Details), new { id = productId });
        }
        public async Task<IActionResult> Compare(int[] ids)
        {
            if (ids == null || ids.Length < 2)
            {
                TempData["Error"] = "Please select at least 2 products to compare.";
                return RedirectToAction(nameof(Index));
            }
            var products = new List<Product>();
            foreach (var id in ids.Distinct().Take(4))
            {
                var product = await _productService.GetProductByIdAsync(id);
                if (product != null)
                {
                    products.Add(product);
                }
            }
            if (products.Count < 2)
            {
                TempData["Error"] = "Compare requires minimum 2 valid products.";
                return RedirectToAction(nameof(Index));
            }
            var specificationNames = products
                .SelectMany(p => p.Specifications)
                .Select(s => s.Name)
                .Distinct()
                .ToList();

            var viewModel = new CompareProductsViewModel
            {
                Products = products,
                SpecificationNames = specificationNames
            };
            ViewBag.CartItemCount = await _cartService.GetCartItemCountAsync();
            return View(viewModel);
        }
       
    }
}
