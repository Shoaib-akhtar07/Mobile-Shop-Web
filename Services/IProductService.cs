using Microsoft.EntityFrameworkCore;
using MobileShop.Data;
using MobileShop.Models;
using MobileShop.ViewModels;
using Stripe.Tax;

namespace MobileShop.Services
{
    public interface IProductService
    {
        // Fetches a single product by its primary key
        Task<Product?> GetProductByIdAsync(int id);

        // Fetches a list of similar products based on Category or Brand
        // Default count is set to 4 for a standard UI row
        Task<List<Product>> GetRelatedProductsAsync(int productId, int count = 4);
        // Asynchronously gets a filtered, sorted, and paginated list of products
        Task<ProductListViewModel> GetProductsAsync(ProductListViewModel filter);
        Task<List<Product>> GetFeaturedProductsAsync(int count = 8);
        Task<List<Product>> GetNewArrivalsAsync(int count = 8);
        Task<List<Product>> GetBestsellersAsync(int count = 8);
        Task<List<Brand>> GetBrandsAsync(int count = 6);
        Task<bool> AddReviewAsync(int productId, string userId, ReviewViewModel review);
    }
    public class ProductService : IProductService
    {
        private readonly ApplicationDbContext _context;

        public ProductService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Product>> GetBestsellersAsync(int count = 8)
        {
            return await _context.Products
                .Include(p => p.Brand)
                .Include(p => p.Reviews)
                .Where(p => p.IsBestseller && p.IsActive)
                .Take(count)
                .ToListAsync();
        }

        public async Task<List<Product>> GetFeaturedProductsAsync(int count = 8)
        {
            return await _context.Products
                .Include(p => p.Brand)
                .Include(p => p.Reviews)
                .Where(p => p.IsFeatured && p.IsActive)
                .Take(count)
                .ToListAsync();
        }

        public async Task<List<Product>> GetNewArrivalsAsync(int count = 8)
        {
            return await _context.Products
                .Include(p => p.Brand)
                .Include(p => p.Reviews)
                .Where(p => p.IsNewArrival && p.IsActive)
                .OrderByDescending(p => p.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<List<Brand>> GetBrandsAsync(int count = 6)
        {
            return await _context.Brands.Where(b => b.IsActive == true).Take(count).ToListAsync();
        }

        public async Task<ProductListViewModel> GetProductsAsync(ProductListViewModel filter)
        {
            // 1. Initialize query with related data
            var query = _context.Products
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .Include(p => p.Reviews)
                .Where(p => p.IsActive)
                .AsQueryable();
            if (filter.SelectedCategoryId.HasValue)
                query = query.Where(p => p.CategoryId == filter.SelectedCategoryId.Value);

            if (filter.SelectedBrandId.HasValue)
                query = query.Where(p => p.BrandId == filter.SelectedBrandId.Value);

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
                query = query.Where(p => p.Name.Contains(filter.SearchTerm) || 
                                         p.Description.Contains(filter.SearchTerm) ||
                                         p.Brand.Name.Contains(filter.SearchTerm));
            if (filter.MinPrice.HasValue)
                query = query.Where(p => p.SalePrice >= filter.MinPrice.Value);

            if (filter.MaxPrice.HasValue)
                query = query.Where(p => p.SalePrice <= filter.MaxPrice.Value);

            // 3. Apply Sorting using C# Switch Expression
            query = filter.SortOrder?.ToLower() switch
            {
                "price_asc" => query.OrderBy(p => p.SalePrice),
                "price_desc" => query.OrderByDescending(p => p.SalePrice),
                "name_asc" => query.OrderBy(p => p.Name),
                "name_desc" => query.OrderByDescending(p => p.Name),
                "newest" => query.OrderByDescending(p => p.CreatedAt),
                _ => query.OrderByDescending(p => p.CreatedAt)
            };

            // 4. Calculate Pagination Stats
            filter.TotalCount = await query.CountAsync();
            filter.TotalPages = (int)Math.Ceiling(filter.TotalCount / (double)filter.PageSize);

            // 5. Execute Query with Skip/Take
            filter.Products = await query
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            // 6. Populate filter dropdowns
            filter.Categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
            filter.Brands = await _context.Brands.Where(b => b.IsActive).ToListAsync();

            return filter;
        

        }

        public async Task<Product?> GetProductByIdAsync(int id)
        {
            // Fetches the product with all its related details
            return await _context.Products
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .Include(p => p.Specifications)
                .Include(p => p.Reviews)
                    .ThenInclude(r => r.User) // Deep loading the user who wrote the review
                .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);
        }

        public async Task<List<Product>> GetRelatedProductsAsync(int productId, int count = 4)
        {
            // 1. Fetch the base product to get its Brand and Category
            var product = await _context.Products.FindAsync(productId);

            // Return empty list if product doesn't exist
            if (product == null) return new List<Product>();

            // 2. Query for similar products
            return await _context.Products
                .Include(p => p.Brand)
                .Where(p => p.Id != productId &&      // Exclude the current product
                            (p.BrandId == product.BrandId || p.CategoryId == product.CategoryId) &&
                            p.IsActive)               // Only show active items
                .OrderBy(x => Guid.NewGuid())        // Optional: Randomize the results
                .Take(count)                          // Limit the result set
                .ToListAsync();
        }

        public async Task<bool> AddReviewAsync(int productId, string userId, ReviewViewModel review)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null) return false;

            var newReview = new Review
            {
                ProductId = productId,
                UserId = userId,
                Rating = review.Rating,
                Title = review.Title,
                Comment = review.Comment,
                IsApproved = true // Auto-approve for now
            };

            _context.Reviews.Add(newReview);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
