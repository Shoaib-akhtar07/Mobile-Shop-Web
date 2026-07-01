using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MobileShop.Models
{
    public class WishlistItem
    {
        [Key]
        public int Id { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime AddedAt { get; set; } = DateTime.Now;

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
    }
}