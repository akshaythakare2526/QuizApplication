using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuizApplication.Data;
using QuizApplication.Models;

namespace QuizApplication.Controllers
{
    public class QuestionsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public QuestionsController(ApplicationDbContext context)
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

        // GET: Questions - Display all questions (Admin only)
        public async Task<IActionResult> Index()
        {
            var redirectResult = RedirectIfNotAuthenticated();
            if (redirectResult != null) return redirectResult;

            if (!IsUserAdmin())
            {
                TempData["Error"] = "Access denied. Admin privileges required.";
                return RedirectToAction("Dashboard", "Users");
            }

            var questions = await _context.Questions
                .Include(q => q.Category)
                .OrderBy(q => q.CategoryId)
                .ThenBy(q => q.QuestionId)
                .ToListAsync();

            return View(questions);
        }

        // GET: Questions/Details/5
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

            var question = await _context.Questions
                .Include(q => q.Category)
                .FirstOrDefaultAsync(m => m.QuestionId == id);

            if (question == null)
            {
                return NotFound();
            }

            return View(question);
        }

        // GET: Questions/Create
        public async Task<IActionResult> Create()
        {
            var redirectResult = RedirectIfNotAuthenticated();
            if (redirectResult != null) return redirectResult;

            if (!IsUserAdmin())
            {
                TempData["Error"] = "Access denied. Admin privileges required.";
                return RedirectToAction("Dashboard", "Users");
            }

            var categories = await _context.Categories.ToListAsync();
            ViewBag.CategoryId = new SelectList(categories, "CategoryId", "CategoryName");
            ViewBag.DifficultyLevels = new SelectList(new[]
            {
                new { Value = "Easy", Text = "Easy" },
                new { Value = "Medium", Text = "Medium" },
                new { Value = "Hard", Text = "Hard" }
            }, "Value", "Text");

            ViewBag.CorrectOptions = new SelectList(new[]
            {
                new { Value = "Option1", Text = "Option A" },
                new { Value = "Option2", Text = "Option B" },
                new { Value = "Option3", Text = "Option C" },
                new { Value = "Option4", Text = "Option D" }
            }, "Value", "Text");

            return View();
        }

        // POST: Questions/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Questions question)
        {
            var redirectResult = RedirectIfNotAuthenticated();
            if (redirectResult != null) return redirectResult;

            if (!IsUserAdmin())
            {
                TempData["Error"] = "Access denied. Admin privileges required.";
                return RedirectToAction("Dashboard", "Users");
            }

            // Handle image upload
            if (question.ImageFile != null && question.ImageFile.Length > 0)
            {
                // Validate file type
                var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif" };
                if (!allowedTypes.Contains(question.ImageFile.ContentType.ToLower()))
                {
                    ModelState.AddModelError("ImageFile", "Only JPEG, PNG, and GIF images are allowed.");
                }
                // Validate file size (max 5MB)
                else if (question.ImageFile.Length > 5 * 1024 * 1024)
                {
                    ModelState.AddModelError("ImageFile", "Image size cannot exceed 5MB.");
                }
                else
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await question.ImageFile.CopyToAsync(memoryStream);
                        question.QuestionImageData = memoryStream.ToArray();
                        question.ImageContentType = question.ImageFile.ContentType;
                    }
                }
            }

            if (ModelState.IsValid)
            {
                _context.Add(question);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Question created successfully!";
                return RedirectToAction(nameof(Index));
            }

            // Repopulate dropdowns if validation fails
            var categories = await _context.Categories.ToListAsync();
            ViewBag.CategoryId = new SelectList(categories, "CategoryId", "CategoryName", question.CategoryId);
            ViewBag.DifficultyLevels = new SelectList(new[]
            {
                new { Value = "Easy", Text = "Easy" },
                new { Value = "Medium", Text = "Medium" },
                new { Value = "Hard", Text = "Hard" }
            }, "Value", "Text", question.DifficultyLevel);

            ViewBag.CorrectOptions = new SelectList(new[]
            {
                new { Value = "Option1", Text = "Option A" },
                new { Value = "Option2", Text = "Option B" },
                new { Value = "Option3", Text = "Option C" },
                new { Value = "Option4", Text = "Option D" }
            }, "Value", "Text", question.CorrectedOption);

            return View(question);
        }

        // GET: Questions/Edit/5
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

            var question = await _context.Questions.FindAsync(id);
            if (question == null)
            {
                return NotFound();
            }

            var categories = await _context.Categories.ToListAsync();
            ViewBag.CategoryId = new SelectList(categories, "CategoryId", "CategoryName", question.CategoryId);
            ViewBag.DifficultyLevels = new SelectList(new[]
            {
                new { Value = "Easy", Text = "Easy" },
                new { Value = "Medium", Text = "Medium" },
                new { Value = "Hard", Text = "Hard" }
            }, "Value", "Text", question.DifficultyLevel);

            ViewBag.CorrectOptions = new SelectList(new[]
            {
                new { Value = "Option1", Text = "Option A" },
                new { Value = "Option2", Text = "Option B" },
                new { Value = "Option3", Text = "Option C" },
                new { Value = "Option4", Text = "Option D" }
            }, "Value", "Text", question.CorrectedOption);

            return View(question);
        }

        // POST: Questions/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Questions question)
        {
            var redirectResult = RedirectIfNotAuthenticated();
            if (redirectResult != null) return redirectResult;

            if (!IsUserAdmin())
            {
                TempData["Error"] = "Access denied. Admin privileges required.";
                return RedirectToAction("Dashboard", "Users");
            }

            if (id != question.QuestionId)
            {
                return NotFound();
            }

            // Get existing question to preserve image if no new image is uploaded
            var existingQuestion = await _context.Questions.AsNoTracking().FirstOrDefaultAsync(q => q.QuestionId == id);
            if (existingQuestion == null)
            {
                return NotFound();
            }

            // Handle image upload
            if (question.ImageFile != null && question.ImageFile.Length > 0)
            {
                // Validate file type
                var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif" };
                if (!allowedTypes.Contains(question.ImageFile.ContentType.ToLower()))
                {
                    ModelState.AddModelError("ImageFile", "Only JPEG, PNG, and GIF images are allowed.");
                }
                // Validate file size (max 5MB)
                else if (question.ImageFile.Length > 5 * 1024 * 1024)
                {
                    ModelState.AddModelError("ImageFile", "Image size cannot exceed 5MB.");
                }
                else
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await question.ImageFile.CopyToAsync(memoryStream);
                        question.QuestionImageData = memoryStream.ToArray();
                        question.ImageContentType = question.ImageFile.ContentType;
                    }
                }
            }
            else
            {
                // Keep existing image data if no new image is uploaded
                question.QuestionImageData = existingQuestion.QuestionImageData;
                question.ImageContentType = existingQuestion.ImageContentType;
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(question);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Question updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!QuestionExists(question.QuestionId))
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

            // Repopulate dropdowns if validation fails
            var categories = await _context.Categories.ToListAsync();
            ViewBag.CategoryId = new SelectList(categories, "CategoryId", "CategoryName", question.CategoryId);
            ViewBag.DifficultyLevels = new SelectList(new[]
            {
                new { Value = "Easy", Text = "Easy" },
                new { Value = "Medium", Text = "Medium" },
                new { Value = "Hard", Text = "Hard" }
            }, "Value", "Text", question.DifficultyLevel);

            ViewBag.CorrectOptions = new SelectList(new[]
            {
                new { Value = "Option1", Text = "Option A" },
                new { Value = "Option2", Text = "Option B" },
                new { Value = "Option3", Text = "Option C" },
                new { Value = "Option4", Text = "Option D" }
            }, "Value", "Text", question.CorrectedOption);

            return View(question);
        }

        // GET: Questions/Delete/5
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

            var question = await _context.Questions
                .Include(q => q.Category)
                .FirstOrDefaultAsync(m => m.QuestionId == id);

            if (question == null)
            {
                return NotFound();
            }

            return View(question);
        }

        // POST: Questions/Delete/5
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

            var question = await _context.Questions.FindAsync(id);
            if (question != null)
            {
                _context.Questions.Remove(question);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Question deleted successfully!";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Questions/GetImage/5 - Serve image from database
        public async Task<IActionResult> GetImage(int id)
        {
            var question = await _context.Questions.FindAsync(id);
            if (question?.QuestionImageData == null)
            {
                return NotFound();
            }

            return File(question.QuestionImageData, question.ImageContentType ?? "image/jpeg");
        }

        // POST: Questions/RemoveImage/5 - Remove image from question
        [HttpPost]
        public async Task<IActionResult> RemoveImage(int id)
        {
            var redirectResult = RedirectIfNotAuthenticated();
            if (redirectResult != null) return redirectResult;

            if (!IsUserAdmin())
            {
                return Json(new { success = false, message = "Access denied. Admin privileges required." });
            }

            var question = await _context.Questions.FindAsync(id);
            if (question == null)
            {
                return Json(new { success = false, message = "Question not found." });
            }

            question.QuestionImageData = null;
            question.ImageContentType = null;

            try
            {
                _context.Update(question);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Image removed successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error removing image: " + ex.Message });
            }
        }

        private bool QuestionExists(int id)
        {
            return _context.Questions.Any(e => e.QuestionId == id);
        }
    }
}
