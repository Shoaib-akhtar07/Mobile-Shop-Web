using MobileShop.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MobileShop.ViewModels
{
    public class ProductListViewModel
    {
        // Data Lists
        public List<Product> Products { get; set; } = new List<Product>();
        public List<Category> Categories { get; set; } = new List<Category>();
        public List<Brand> Brands { get; set; } = new List<Brand>();

        // Filter Parameters
        public int? SelectedCategoryId { get; set; }
        public int? SelectedBrandId { get; set; }
        public string? SearchTerm { get; set; }
        public string? SortOrder { get; set; }

        // Price Range Filtering
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }

        // Pagination Properties
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 12;
        public int TotalPages { get; set; }
        public int TotalCount { get; set; }
    }
    public class ProductDetailViewModel
    {
        // The main product being viewed
        public Product Product { get; set; } = null!;

        // A list of similar products (e.g., from the same brand or category)
        public List<Product> RelatedProducts { get; set; } = new List<Product>();

        // Used to toggle the heart icon if the user has already saved this item
        public bool IsInWishlist { get; set; }
    }
  
    public class ReviewViewModel
    {
        [Required]
        [Range(1, 5, ErrorMessage = "Please select a rating between 1 and 5")]
        public int Rating { get; set; }

        [StringLength(100)]
        public string? Title { get; set; }

        [StringLength(1000)]
        public string? Comment { get; set; }
    }
    public class CompareProductsViewModel
    {
        public List<Product> Products { get; set; } = new();
        public List<string> SpecificationNames { get; set; } = new();
    }
}