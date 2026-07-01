using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MobileShop.Data;
using MobileShop.Models;
using MobileShop.Services;

namespace YourProjectNamespace.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileService _fileService;

        public CategoriesController(ApplicationDbContext context , IFileService fileService)
        {
            _context = context;
            _fileService = fileService;
        }
        // GET: Admin/Categories
        public async Task<IActionResult> Index()
        {
            // Eagerly load Products to avoid NullReferenceException in the View count badge
            var categories = await _context.Categories
                                           .Include(c => c.Products)
                                           .OrderBy(c => c.DisplayOrder)
                                           .ToListAsync();

            return View(categories);
        }
        // POST: Admin/Categories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category category)
        {
            if (ModelState.IsValid)
            {
                if (category.CategoryImage != null)
                {
                    var imagePath = await _fileService.SaveFileAsync(category.CategoryImage, "images/category");
                    if (!string.IsNullOrEmpty(imagePath))
                    {
                        if (!string.IsNullOrEmpty(category.ImageUrl))
                            _fileService.DeleteFile(category.ImageUrl);
                        category.ImageUrl = imagePath;
                    }
                }
                _context.Categories.Add(category);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Category created successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }
        // GET: Admin/Categories/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
                return NotFound();

            return View(category);
        }

        // POST: Admin/Categories/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Category category)
        {
            if (id != category.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    if (category.CategoryImage != null)
                    {
                        var imagePath = await _fileService.SaveFileAsync(category.CategoryImage, "images/category");
                        if (!string.IsNullOrEmpty(imagePath))
                        {
                            if (!string.IsNullOrEmpty(category.ImageUrl))
                                _fileService.DeleteFile(category.ImageUrl);
                            category.ImageUrl = imagePath;
                        }
                    }
                    _context.Update(category);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Category updated successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if ( !CategoryExists(category.Id))
                    {
                        return NotFound();
                    }

                    throw;
                }
            }
            return View(category);
        }

        
       
        // POST: Admin/Categories/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
                return NotFound();

            // Soft delete implementation: Flagging rather than destructive removing
            category.IsActive = false;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Category deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        // Helper method used to verify existence during concurrency conflicts
        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.Id == id);
        }
    }
}
