
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MobileShop.Data;
using MobileShop.Models;
using MobileShop.ViewModels;

namespace MobileShop.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;

        public UsersController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }
        public async Task<IActionResult> Index(string? search, string? role, int page = 1)
        {
            var query = _userManager.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(u => u.Email.Contains(search) ||
                                        u.FirstName.Contains(search) ||
                                        u.LastName.Contains(search));

            var users = await query
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();

            var userViewModels = new List<UserManagementViewModel>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var orderCount = await _context.Orders.CountAsync(o => o.UserId == user.Id);

                if (!string.IsNullOrWhiteSpace(role) && !roles.Contains(role))
                    continue;

                userViewModels.Add(new UserManagementViewModel
                {
                    UserId = user.Id,
                    FullName = user.FullName,
                    Email = user.Email!,
                    PhoneNumber = user.PhoneNumber ?? "",
                    Roles = roles.ToList(),
                    IsActive = user.IsActive,
                    CreatedAt = user.CreatedAt,
                    OrderCount = orderCount
                });
            }

            var pageSize = 20;
            var totalItems = userViewModels.Count;
            var pagedUsers = userViewModels
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.Roles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
            ViewBag.CurrentRole = role;
            ViewBag.Search = search;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            return View(pagedUsers);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleStatus(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            user.IsActive = !user.IsActive;
            await _userManager.UpdateAsync(user);

            var status = user.IsActive ? "activated" : "deactivated";
            TempData["Success"] = $"User {status} successfully.";

            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Details(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            var roles = await _userManager.GetRolesAsync(user);
            var orders = await _context.Orders
                .Where(o => o.UserId == id)
                .OrderByDescending(o => o.OrderDate)
                .Take(10)
                .ToListAsync();

            ViewBag.Roles = roles;
            ViewBag.Orders = orders;

            return View(user);
        }
        [HttpPost]
        public async Task<IActionResult> UpdateRole(string id, string role, bool add)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            if (add)
            {
                if (!await _userManager.IsInRoleAsync(user, role))
                {
                    await _userManager.AddToRoleAsync(user, role);
                    TempData["Success"] = $"Role '{role}' added to user.";
                }
            }
            else
            {
                if (await _userManager.IsInRoleAsync(user, role))
                {
                    await _userManager.RemoveFromRoleAsync(user, role);
                    TempData["Success"] = $"Role '{role}' removed from user.";
                }
            }

            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
