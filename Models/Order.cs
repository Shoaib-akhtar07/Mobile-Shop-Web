using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MobileShop.Models
{
    public enum OrderStatus
    {
        Pending,
        Processing,
        Shipped,
        Delivered,
        Cancelled,
        Refunded
    }

    public enum PaymentStatus
    {
        Pending,
        Paid,
        Failed,
        Refunded
    }

    public enum PaymentMethod
    {
        CreditCard,
        DebitCard,
        PayPal,
        Stripe,
        CashOnDelivery,
        UPI
    }

    public class Order
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(35)]
        [Display(Name = "Order Number")]
        public string OrderNumber { get; set; } = GenerateOrderNumber();

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Subtotal")]
        public decimal Subtotal { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Tax Amount")]
        public decimal TaxAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Shipping Cost")]
        public decimal ShippingCost { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Discount Amount")]
        public decimal DiscountAmount { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Total Amount")]
        public decimal TotalAmount { get; set; }

        [Display(Name = "Order Status")]
        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        [Display(Name = "Payment Status")]
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;

        [Display(Name = "Payment Method")]
        public PaymentMethod PaymentMethod { get; set; }

        [StringLength(100)]
        [Display(Name = "Transaction ID")]
        public string? TransactionId { get; set; }

        // Shipping Address
        [Required]
        [StringLength(200)]
        [Display(Name = "Shipping Address")]
        public string ShippingAddress { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [Display(Name = "City")]
        public string ShippingCity { get; set; } = string.Empty;

        [StringLength(20)]
        [Display(Name = "Postal Code")]
        public string? ShippingPostalCode { get; set; }

        [StringLength(50)]
        [Display(Name = "Country")]
        public string ShippingCountry { get; set; } = "Pakistan";

        [StringLength(20)]
        [Display(Name = "Phone Number")]
        public string? ShippingPhone { get; set; }

        [StringLength(500)]
        [Display(Name = "Order Notes")]
        public string? Notes { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Name = "Order Date")]
        public DateTime OrderDate { get; set; } = DateTime.Now;

        [DataType(DataType.DateTime)]
        [Display(Name = "Shipped Date")]
        public DateTime? ShippedDate { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Name = "Delivered Date")]
        public DateTime? DeliveredDate { get; set; }

        // Foreign Key
        [Display(Name = "Customer")]
        public string? UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }

        // Navigation Properties
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

        private static string GenerateOrderNumber()
        {
            return $"ORD-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
        }
    }
}
