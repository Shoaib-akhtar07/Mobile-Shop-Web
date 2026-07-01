using Microsoft.AspNetCore.Mvc;
using MobileShop.Data;
using System.Linq;

namespace MobileShop.ViewComponents
{
    namespace MobileShop.ViewComponents
    {
        public class BrandsMenuViewComponent : ViewComponent
        {
            private readonly ApplicationDbContext _context;

            public BrandsMenuViewComponent(ApplicationDbContext context)
            {
                _context = context;
            }

            public IViewComponentResult Invoke()
            {
                // Fetching only active brands from the database
                var brands = _context.Brands
                                     .Where(b => b.IsActive)
                                     .OrderBy(b => b.Name)
                                     .ToList();

                return View(brands);

            }
        }
    }
}