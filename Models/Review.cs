using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MobileShop.Models
{
    public class Review
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }

        [StringLength(1000)]
        public string? Comment { get; set; }

        [StringLength(100)]
        public string? Title { get; set; }

        public bool IsVerifiedPurchase { get; set; } = false;
        public bool IsApproved { get; set; } = false;

        // Foreign Keys
        [Display(Name = "Product")]
        public int ProductId { get; set; }

        [Display(Name = "User")]
        public string UserId { get; set; } = string.Empty;

        // Navigation Properties
        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; } = null!;

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; } = null!;

        [DataType(DataType.DateTime)]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}