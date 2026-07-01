using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MobileShop.Data;
using MobileShop.Models;
using MobileShop.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MobileShop.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class BrandsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileService _fileService;


        public BrandsController(ApplicationDbContext context , IFileService fileService)
        {
            _context = context;
            _fileService = fileService;
        }

        // GET: Admin/Brands
        public async Task<IActionResult> Index()
        {
            return View(await _context.Brands
                .ToListAsync());
        }

        // GET: Admin/Brands/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var brand = await _context.Brands
                .FirstOrDefaultAsync(m => m.Id == id);
            if (brand == null)
            {
                return NotFound();
            }

            return View(brand);
        }

        // GET: Admin/Brands/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Brands/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Brand brand)
        {
            if (ModelState.IsValid)
            {
                if (brand.BrandLogo != null)
                {
                    var imagePath = await _fileService.SaveFileAsync(brand.BrandLogo, "images/brand");
                    if (!string.IsNullOrEmpty(imagePath))
                    {
                        if (!string.IsNullOrEmpty(brand.LogoUrl))
                            _fileService.DeleteFile(brand.LogoUrl);
                        brand.LogoUrl = imagePath;
                    }
                }
                _context.Add(brand);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(brand);
        }

        // GET: Admin/Brands/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var brand = await _context.Brands.FindAsync(id);
            if (brand == null)
            {
                return NotFound();
            }
            return View(brand);
        }

        // POST: Admin/Brands/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Brand brand)
        {
            if (id != brand.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (brand.BrandLogo != null)
                    {
                        var imagePath = await _fileService.SaveFileAsync(brand.BrandLogo, "images/brands");
                        if (!string.IsNullOrEmpty(imagePath))
                        {
                            if (!string.IsNullOrEmpty(brand.LogoUrl))
                                _fileService.DeleteFile(brand.LogoUrl);
                            brand.LogoUrl = imagePath;
                        }
                    }
                    _context.Update(brand);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Brand Updated sucessfully";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BrandExists(brand.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        TempData["Error"] = "Again Login and retry";
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(brand);
        }

        

        // POST: Admin/Brands/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var brand = await _context.Brands.FindAsync(id);
            if (brand != null)
            {
                brand.IsActive=false;
            }

            await _context.SaveChangesAsync();
            TempData["Delete"] = "Deleted successfully";
            return RedirectToAction(nameof(Index));
        }

        private bool BrandExists(int id)
        {
            return _context.Brands.Any(e => e.Id == id);
        }
    }
}
