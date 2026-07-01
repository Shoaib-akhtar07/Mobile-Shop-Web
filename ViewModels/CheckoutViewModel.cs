using MobileShop.Models;
using System.ComponentModel.DataAnnotations;

namespace MobileShop.ViewModels
{
    public class CheckoutViewModel
    {
        public ShoppingCartViewModel Cart { get; set; } = new ShoppingCartViewModel();

        // Shipping Information
        [Required(ErrorMessage = "First name is required")]
        [Display(Name = "First Name")]
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        [Display(Name = "Last Name")]
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required")]
        [Phone]
        [Display(Name = "Phone Number")]
        [StringLength(20)]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Address is required")]
        [Display(Name = "Street Address")]
        [StringLength(200)]
        public string Address { get; set; } = string.Empty;

        [StringLength(100)]
        [Display(Name = "Apartment, Suite, etc.")]
        public string? Address2 { get; set; }

        [Required(ErrorMessage = "City is required")]
        [Display(Name = "City")]
        [StringLength(100)]
        public string City { get; set; } = string.Empty;

        [Display(Name = "State / Province")]
        [StringLength(100)]
        public string? State { get; set; }

        [Required(ErrorMessage = "Postal code is required")]
        [Display(Name = "Postal Code")]
        [StringLength(20)]
        public string PostalCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Country is required")]
        [Display(Name = "Country")]
        [StringLength(50)]
        public string Country { get; set; } = "Pakistan";

        [StringLength(500)]
        [Display(Name = "Order Notes")]
        [DataType(DataType.MultilineText)]
        public string? OrderNotes { get; set; }

        // Payment Information
        [Required(ErrorMessage = "Please select a payment method")]
        [Display(Name = "Payment Method")]
        public PaymentMethod PaymentMethod { get; set; }

        // For Stripe
        public string? StripeToken { get; set; }

        // Save address for future
        [Display(Name = "Save this address for future orders")]
        public bool SaveAddress { get; set; }

        public bool IsAuthenticated { get; set; }
        public List<SavedAddressViewModel>? SavedAddresses { get; set; }
    }

    public class SavedAddressViewModel
    {
        public int Id { get; set; }
        public string Label { get; set; } = string.Empty;
        public string FullAddress { get; set; } = string.Empty;
    }
    public class OrderConfirmationViewModel
    {
        public Order Order { get; set; } = null!;
        public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }

}