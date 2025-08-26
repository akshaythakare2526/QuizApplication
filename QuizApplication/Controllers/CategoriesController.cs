using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuizApplication.Data;
using QuizApplication.Models;

namespace QuizApplication.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Check if user is admin - helper method
        private bool IsUserAdmin()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");
            return userId != null && userRole == "Admin";
        }

        // Redirect to login if not authenticated
        private IActionResult RedirectIfNotAuthenticated()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Index", "Users");
            }
            return null;
        }

        // GET: Categories - Display all categories (Admin only)
        public async Task<IActionResult> Index()
        {
            var redirectResult = RedirectIfNotAuthenticated();
            if (redirectResult != null) return redirectResult;

            if (!IsUserAdmin())
            {
                TempData["Error"] = "Access denied. Admin privileges required.";
                return RedirectToAction("Dashboard", "Users");
            }

            var categories = await _context.Categories.ToListAsync();
            return View(categories);
        }

        // GET: Categories/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            var redirectResult = RedirectIfNotAuthenticated();
            if (redirectResult != null) return redirectResult;

            if (!IsUserAdmin())
            {
                TempData["Error"] = "Access denied. Admin privileges required.";
                return RedirectToAction("Dashboard", "Users");
            }

            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(m => m.CategoryId == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // GET: Categories/Create
        public IActionResult Create()
        {
            var redirectResult = RedirectIfNotAuthenticated();
            if (redirectResult != null) return redirectResult;

            if (!IsUserAdmin())
            {
                TempData["Error"] = "Access denied. Admin privileges required.";
                return RedirectToAction("Dashboard", "Users");
            }

            return View();
        }

        // POST: Categories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CategoryId,CategoryName")] Categories category)
        {
            var redirectResult = RedirectIfNotAuthenticated();
            if (redirectResult != null) return redirectResult;

            if (!IsUserAdmin())
            {
                TempData["Error"] = "Access denied. Admin privileges required.";
                return RedirectToAction("Dashboard", "Users");
            }

            if (ModelState.IsValid)
            {
                // Check if category name already exists
                var existingCategory = await _context.Categories
                    .FirstOrDefaultAsync(c => c.CategoryName.ToLower() == category.CategoryName.ToLower());
                
                if (existingCategory != null)
                {
                    ModelState.AddModelError("CategoryName", "A category with this name already exists.");
                    return View(category);
                }

                _context.Add(category);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Category created successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // GET: Categories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            var redirectResult = RedirectIfNotAuthenticated();
            if (redirectResult != null) return redirectResult;

            if (!IsUserAdmin())
            {
                TempData["Error"] = "Access denied. Admin privileges required.";
                return RedirectToAction("Dashboard", "Users");
            }

            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        // POST: Categories/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CategoryId,CategoryName")] Categories category)
        {
            var redirectResult = RedirectIfNotAuthenticated();
            if (redirectResult != null) return redirectResult;

            if (!IsUserAdmin())
            {
                TempData["Error"] = "Access denied. Admin privileges required.";
                return RedirectToAction("Dashboard", "Users");
            }

            if (id != category.CategoryId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // Check if category name already exists (excluding current category)
                var existingCategory = await _context.Categories
                    .FirstOrDefaultAsync(c => c.CategoryName.ToLower() == category.CategoryName.ToLower() 
                                            && c.CategoryId != category.CategoryId);
                
                if (existingCategory != null)
                {
                    ModelState.AddModelError("CategoryName", "A category with this name already exists.");
                    return View(category);
                }

                try
                {
                    _context.Update(category);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Category updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(category.CategoryId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // GET: Categories/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            var redirectResult = RedirectIfNotAuthenticated();
            if (redirectResult != null) return redirectResult;

            if (!IsUserAdmin())
            {
                TempData["Error"] = "Access denied. Admin privileges required.";
                return RedirectToAction("Dashboard", "Users");
            }

            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(m => m.CategoryId == id);
            if (category == null)
            {
                return NotFound();
            }

            // Check if category has associated questions
            var hasQuestions = await _context.Questions.AnyAsync(q => q.CategoryId == id);
            ViewBag.HasQuestions = hasQuestions;

            return View(category);
        }

        // POST: Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var redirectResult = RedirectIfNotAuthenticated();
            if (redirectResult != null) return redirectResult;

            if (!IsUserAdmin())
            {
                TempData["Error"] = "Access denied. Admin privileges required.";
                return RedirectToAction("Dashboard", "Users");
            }

            var category = await _context.Categories.FindAsync(id);
            if (category != null)
            {
                // Check if category has associated questions
                var hasQuestions = await _context.Questions.AnyAsync(q => q.CategoryId == id);
                if (hasQuestions)
                {
                    TempData["Error"] = "Cannot delete category. It has associated questions. Please delete or reassign the questions first.";
                    return RedirectToAction(nameof(Index));
                }

                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Category deleted successfully!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.CategoryId == id);
        }
    }
}
