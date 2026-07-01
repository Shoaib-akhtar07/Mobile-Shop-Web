using Stripe;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MobileShop.Models
{
    public class Brand
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Brand Name")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

    
        [Display(Name = "Logo URL")]
        [NotMapped]
        public IFormFile? BrandLogo { get; set; }
        public string? LogoUrl { get; set; }

        [StringLength(100)]
        public string? Country { get; set; }

        [StringLength(255)]
        [Display(Name = "Official Website")]
        public string? WebsiteUrl { get; set; }

        public bool IsActive { get; set; } = true;

        [DataType(DataType.DateTime)]
        public DateTime CreatedAt { get; set; } =DateTime.Now;

        // Navigation Properties
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    }
}