using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MobileShop.Data;
using MobileShop.Models;
using MobileShop.Services;

namespace MobileShop.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileService _fileService;

        public ProductsController(ApplicationDbContext context, IFileService fileService)
        {
            _context = context;
            _fileService = fileService;
        }
        public async Task<IActionResult> Index(string? search, int? categoryId, int? brandId, int page = 1)
        {
            // Build deferred execution query including relationships
            var query = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .AsQueryable();
          
            // Apply Search Filtering
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(p => p.Name.Contains(search) || p.Model.Contains(search));
            }
                 
            // Apply Category Filtering
            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            // Apply Brand Filtering
            if (brandId.HasValue)
            {
                query = query.Where(p => p.BrandId == brandId.Value);
            }

            // Pagination Settings
            var pageSize = 11;
            var totalItems = await query.CountAsync();

            var products = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Populate Lookup Data and State for the View
            ViewBag.Categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
            ViewBag.Brands = await _context.Brands.Where(b => b.IsActive).ToListAsync();
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            ViewBag.Search = search;

            return View(products);
        }
        // GET: Admin/Products/Create
        public async Task<IActionResult> Create()
        {
            // Populate lookup collections for lookups/drop-downs before loading the form
            ViewBag.Categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
            ViewBag.Brands = await _context.Brands.Where(b => b.IsActive).ToListAsync();

            return View();
        }
        // POST: Admin/Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Model,CategoryId,BrandId,OriginalPrice,SalePrice,StockQuantity,ShortDescription,Description,IsActive,IsFeatured,IsNewArrival,IsBestseller")] Product product, List<IFormFile> images)
        {
            // Remove EF validation errors for complex navigation properties to prevent false validation failures
            ModelState.Remove("Category");
            ModelState.Remove("Brand");
            ModelState.Remove("ProductImages");
            ModelState.Remove("Specifications");
            ModelState.Remove("Reviews");
            ModelState.Remove("OrderItems");
            ModelState.Remove("WishlistItems");

            // DEBUG: Log server-side model state errors to console/file trackers if invalid
            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState)
                {
                    BadRequest(error);
                }
            }

            if (ModelState.IsValid)
            {
                // Save the primary placeholder/Main image if uploaded
                if (images.Count > 0)
                {
                    product.MainImageUrl = await _fileService.SaveFileAsync(images[0], "images/products");
                }

                _context.Products.Add(product);
                await _context.SaveChangesAsync(); // Generates product.Id for relational storage

                // Loop through and assign additional gallery references
                for (int i = 1; i < images.Count; i++)
                {
                    var imagePath = await _fileService.SaveFileAsync(images[i], "images/products");
                    _context.ProductImages.Add(new ProductImage
                    {
                        ProductId = product.Id,
                        ImageUrl = imagePath,
                        DisplayOrder = i
                    });
                }

                await _context.SaveChangesAsync();
                TempData["Success"] = "Product created successfully.";
                return RedirectToAction(nameof(Index));
            }

            // Repopulate lookup configurations if validation checks fail to prevent form rendering breaking
            ViewBag.Categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
            ViewBag.Brands = await _context.Brands.Where(b => b.IsActive).ToListAsync();
            return View(product);
        }

        // GET: Admin/Products/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            // Eagerly load relational images and specifications lists to populate the editing dashboard view tabs
            var product = await _context.Products
                .Include(p => p.ProductImages)
                .Include(p => p.Specifications)
                .FirstOrDefaultAsync(p => p.Id == id);

            // Return a 404 status error if the target tracking identifier is invalid or missing
            if (product == null)
                return NotFound();

            // Repopulate selective select list structures to maintain drop-down input integrity
            ViewBag.Categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
            ViewBag.Brands = await _context.Brands.Where(b => b.IsActive).ToListAsync();

            return View(product);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Model,CategoryId,BrandId,OriginalPrice,SalePrice,StockQuantity,ShortDescription,Description,IsActive,IsFeatured,IsNewArrival,IsBestseller,CreatedAt,MainImageUrl")] Product product, List<IFormFile> images)
        {
            if (id != product.Id)
                return NotFound();
            // Remove validation errors for navigation properties
            ModelState.Remove("Category");
            ModelState.Remove("Brand");
            ModelState.Remove("ProductImages");
            ModelState.Remove("Specifications");
            ModelState.Remove("Reviews");
            ModelState.Remove("OrderItems");
            ModelState.Remove("WishlistItems");

            // DEBUG: Log all model state errors
            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState)
                {
                    
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Update main image if provided
                    if (images.Count > 0)
                    {
                        if (!string.IsNullOrEmpty(product.MainImageUrl))
                            _fileService.DeleteFile(product.MainImageUrl);
                        product.MainImageUrl = await _fileService.SaveFileAsync(images[0], "images/products");
                    }
                    product.UpdatedAt = DateTime.Now;
                    _context.Update(product);

                    // Save additional images
                    for (int i = 1; i < images.Count; i++)
                    {
                        var imagePath = await _fileService.SaveFileAsync(images[i], "images/products");
                        _context.ProductImages.Add(new ProductImage
                        {
                            ProductId = product.Id,
                            ImageUrl = imagePath,
                            DisplayOrder = i
                        });
                    }

                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Product updated successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.Id))
                        return NotFound();
                    throw;
                }
            }

            ViewBag.Categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
            ViewBag.Brands = await _context.Brands.Where(b => b.IsActive).ToListAsync();
            return View(product);
        }
        // POST: Admin/Products/RemoveSpecification/5
        [HttpPost]
        public async Task<IActionResult> RemoveSpecification(int id)
        {
            // Locate the matching technical requirement entry inside the tracking context asynchronously
            var spec = await _context.ProductSpecifications.FindAsync(id);

            // Safely verify existence to prevent targeting exceptions during state removal phases
            if (spec != null)
            {
                _context.ProductSpecifications.Remove(spec);
                await _context.SaveChangesAsync(); // Commit structural data modifications directly to the database
            }

            // Return a lightweight status verification result payload to confirm data pipeline clearance
            return Json(new { success = true });
        }
        public async Task<IActionResult> Details(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.ProductImages)
                .Include(p => p.Specifications)
                .Include(p => p.Reviews)
                    .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                return NotFound();

            return View(product);
        }

        // GET: Admin/Products/GetLatestProductId
        [HttpGet]
        public async Task<IActionResult> GetLatestProductId()
        {
            // Fetch only the ID of the most recently inserted product to minimize database transmission payload
            var latest = await _context.Products
                .OrderByDescending(p => p.Id)
                .Select(p => new { id = p.Id })
                .FirstOrDefaultAsync();

            // Return the result as a JSON object, defaulting to an ID of 0 if the table is currently empty
            return Json(latest ?? new { id = 0 });
        }
        // POST: Admin/Products/AddSpecification
        [HttpPost]
        public async Task<IActionResult> AddSpecification(int productId, string name, string value, string? groupName)
        {
            // Initialize a new specification entity mapping technical attributes
            var spec = new ProductSpecification
            {
                ProductId = productId,
                Name = name,
                Value = value,
                GroupName = groupName // Optional parameter enabling dynamic categorization groupings
            };

            _context.ProductSpecifications.Add(spec);
            await _context.SaveChangesAsync(); // Persists and generates the database-level spec.Id

            // Return an anonymous JSON payload indicating successful execution state alongside the record identity
            return Json(new { success = true, id = spec.Id });
        }

        // Helper method used to verify product existence within the master context
        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return NotFound();

            // Soft delete
            product.IsActive = false;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Product deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> ExportToExcel()
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Where(p => p.IsActive)
                .ToListAsync();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Products");

            // Headers
            worksheet.Cell(1, 1).Value = "ID";
            worksheet.Cell(1, 2).Value = "Name";
            worksheet.Cell(1, 3).Value = "Model";
            worksheet.Cell(1, 4).Value = "Category";
            worksheet.Cell(1, 5).Value = "Brand";
            worksheet.Cell(1, 6).Value = "Original Price";
            worksheet.Cell(1, 7).Value = "Sale Price";
            worksheet.Cell(1, 8).Value = "Stock";
            worksheet.Cell(1, 9).Value = "Featured";
            worksheet.Cell(1, 10).Value = "Active";

            // Data
            for (int i = 0; i < products.Count; i++)
            {
                var p = products[i];
                worksheet.Cell(i + 2, 1).Value = p.Id;
                worksheet.Cell(i + 2, 2).Value = p.Name;
                worksheet.Cell(i + 2, 3).Value = p.Model;
                worksheet.Cell(i + 2, 4).Value = p.Category?.Name;
                worksheet.Cell(i + 2, 5).Value = p.Brand?.Name;
                worksheet.Cell(i + 2, 6).Value = p.OriginalPrice;
                worksheet.Cell(i + 2, 7).Value = p.SalePrice;
                worksheet.Cell(i + 2, 8).Value = p.StockQuantity;
                worksheet.Cell(i + 2, 9).Value = p.IsFeatured ? "Yes" : "No";
                worksheet.Cell(i + 2, 10).Value = p.IsActive ? "Yes" : "No";
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Products.xlsx");
        }
        [HttpPost]
        public async Task<IActionResult> ImportFromExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["Error"] = "Please select a file.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                using var stream = new MemoryStream();
                await file.CopyToAsync(stream);
                stream.Position = 0;

                using var workbook = new XLWorkbook(stream);
                var worksheet = workbook.Worksheet(1);
                var rows = worksheet.RowsUsed().Skip(1); // Skip header

                foreach (var row in rows)
                {
                    var categoryid = _context.Categories.Where(c => c.Name.Contains(row.Cell(4).GetString())).FirstOrDefault().Id;
                    var brand = row.Cell(5).GetString();
                    var brandid =  0;
                    if (_context.Brands.Where(b => b.Name.Contains(brand.Trim())).ToList().Count() > 0)
                    {
                        brandid = _context.Brands.Where(c => c.Name.Contains(brand.Trim())).FirstOrDefault().Id;
                    }
                    else
                    {
                        var newBrand = new Brand();
                        newBrand.IsActive= true;
                        newBrand.WebsiteUrl = "new" + brand + ".com";
                        newBrand.Country = "";
                        newBrand.Name = brand;                       
                        newBrand.CreatedAt = DateTime.Now;
                        _context.Brands.Add(newBrand);
                        await _context.SaveChangesAsync();
                        brandid = newBrand.Id;
                    }

                    if (_context.Products.Where(p => p.CategoryId == categoryid && p.BrandId == brandid && p.Name == row.Cell(2).GetString() && p.Model == row.Cell(3).GetString()).Count() == 0)
                    {
                       
                    }
                    else
                    {
                        var product = new Product
                        {
                            Name = row.Cell(2).GetString(),
                            Model = row.Cell(3).GetString(),
                            CategoryId = categoryid,
                            BrandId = brandid,
                            OriginalPrice = (decimal)row.Cell(6).GetDouble(),
                            SalePrice = (decimal)row.Cell(7).GetDouble(),
                            StockQuantity = (int)row.Cell(8).GetDouble(),
                            IsFeatured = row.Cell(9).GetString() == "Yes",
                            IsActive = true
                        };
                        _context.Products.Add(product);
                    }
                }

                await _context.SaveChangesAsync();
                TempData["Success"] = "Products imported successfully.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error importing products.";
            }

            return RedirectToAction(nameof(Index));
        }
    }    
}
