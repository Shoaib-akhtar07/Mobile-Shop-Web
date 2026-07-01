using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MobileShop.Models
{
    public class ProductSpecification
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Specification Name")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        [Display(Name = "Specification Value")]
        public string Value { get; set; } = string.Empty;

        [StringLength(50)]
        [Display(Name = "Group Name")]
        public string? GroupName { get; set; }

        [Display(Name = "Display Order")]
        public int DisplayOrder { get; set; }

        // Foreign Key
        [Display(Name = "Product")]
        public int ProductId { get; set; }

        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; } = null!;
    }
}