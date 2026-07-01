using System.ComponentModel.DataAnnotations;

namespace MobileShop.ViewModels
{
       public class ShoppingCartViewModel
        {
            public List<CartItemViewModel> CartItems { get; set; } = new List<CartItemViewModel>();

            [Display(Name = "Cart Total")]
            public decimal CartTotal { get; set; }

            [Display(Name = "Item Count")]
            public int ItemCount { get; set; }

            [Display(Name = "Tax")]
            public decimal Tax { get; set; }

            [Display(Name = "Shipping")]
            public decimal Shipping { get; set; }

            [Display(Name = "Grand Total")]
            public decimal GrandTotal => CartTotal + Tax + Shipping;
        }

        public class CartItemViewModel
        {
            public int CartItemId { get; set; }
            public int ProductId { get; set; }
            public string ProductName { get; set; } = string.Empty;
            public string? ProductImage { get; set; }
            public decimal UnitPrice { get; set; }
            public int Quantity { get; set; }
            public decimal TotalPrice => UnitPrice * Quantity;
            public int StockQuantity { get; set; }
        }
    
}
