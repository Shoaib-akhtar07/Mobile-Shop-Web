using Stripe;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MobileShop.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        [Display(Name = "Product Name")]
        public string Name { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Model { get; set; }

        [StringLength(2000)]
        public string? Description { get; set; }

        [StringLength(1000)]
        [Display(Name = "Short Description")]
        public string? ShortDescription { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Original Price")]
        public decimal OriginalPrice { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Sale Price")]
        public decimal SalePrice { get; set; }

        [Display(Name = "Discount Percentage")]
        public int DiscountPercentage => OriginalPrice > 0
            ? (int)((OriginalPrice - SalePrice) / OriginalPrice * 100)
            : 0;

        [Display(Name = "Stock Quantity")]
        public int StockQuantity { get; set; } = 0;

        [StringLength(255)]
        [Display(Name = "Main Image")]
        public string? MainImageUrl { get; set; }

        public bool IsFeatured { get; set; } = false;
        public bool IsNewArrival { get; set; } = false;
        public bool IsBestseller { get; set; } = false;
        public bool IsActive { get; set; } = true;

        // Foreign Keys
        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        [Display(Name = "Brand")]
        public int BrandId { get; set; }

        // Navigation Properties
        [ForeignKey("CategoryId")]
        public virtual Category Category { get; set; } = null!;

        [ForeignKey("BrandId")]
        public virtual Brand Brand { get; set; } = null!;

        public virtual ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();
        public virtual ICollection<ProductSpecification> Specifications { get; set; } = new List<ProductSpecification>();
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public virtual ICollection<WishlistItem> WishlistItems { get; set; } = new List<WishlistItem>();

        [DataType(DataType.DateTime)]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [DataType(DataType.DateTime)]
        public DateTime? UpdatedAt { get; set; }

        // Computed Properties
        [Display(Name = "Average Rating")]
        public double AverageRating => Reviews.Any() ? Reviews.Average(r => r.Rating) : 0;

        [Display(Name = "Review Count")]
        public int ReviewCount => Reviews.Count;
    }
}
