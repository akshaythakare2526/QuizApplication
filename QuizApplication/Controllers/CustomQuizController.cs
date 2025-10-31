using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuizApplication.Data;
using QuizApplication.Models;
using QuizApplication.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace QuizApplication.Controllers
{
    public class CustomQuizController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CustomQuizController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: CustomQuiz/MyQuizzes - View quizzes created by current user
        public async Task<IActionResult> MyQuizzes()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Index", "Users");
            }

            var quizzes = await _context.UserCustomQuizzes
                .Include(q => q.CreatedBy)
                .Include(q => q.Questions)
                .Include(q => q.Assignments)
                .Where(q => q.CreatedByUserId == userId && q.IsActive)
                .OrderByDescending(q => q.CreatedDate)
                .ToListAsync();

            return View(quizzes);
        }

        // GET: CustomQuiz/AssignedQuizzes - View quizzes assigned to current user
        public async Task<IActionResult> AssignedQuizzes()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Index", "Users");
            }

            var assignments = await _context.UserCustomQuizAssignments
                .Include(a => a.UserQuiz)
                    .ThenInclude(q => q!.CreatedBy)
                .Include(a => a.UserQuiz)
                    .ThenInclude(q => q!.Questions)
                .Where(a => a.AssignedToUserId == userId)
                .OrderByDescending(a => a.AssignedDate)
                .ToListAsync();

            return View(assignments);
        }

        // GET: CustomQuiz/PublicQuizzes - View all public quizzes
        public async Task<IActionResult> PublicQuizzes()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Index", "Users");
            }

            var quizzes = await _context.UserCustomQuizzes
                .Include(q => q.CreatedBy)
                .Include(q => q.Questions)
                .Where(q => q.IsPublic && q.IsActive)
                .OrderByDescending(q => q.CreatedDate)
                .ToListAsync();

            return View(quizzes);
        }

        // GET: CustomQuiz/Create - Show create quiz form
        public IActionResult Create()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Index", "Users");
            }

            return View();
        }

        // POST: CustomQuiz/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Description,TimeLimit,IsPublic")] UserCustomQuiz quiz)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Index", "Users");
            }

            if (ModelState.IsValid)
            {
                quiz.CreatedByUserId = userId.Value;
                quiz.CreatedDate = DateTime.Now;
                quiz.IsActive = true;

                _context.UserCustomQuizzes.Add(quiz);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Quiz created successfully! Now add questions to your quiz.";
                return RedirectToAction("AddQuestions", new { id = quiz.UserQuizId });
            }

            return View(quiz);
        }

        // GET: CustomQuiz/AddQuestions/5
        public async Task<IActionResult> AddQuestions(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Index", "Users");
            }

            var quiz = await _context.UserCustomQuizzes
                .Include(q => q.Questions)
                .FirstOrDefaultAsync(q => q.UserQuizId == id && q.CreatedByUserId == userId);

            if (quiz == null)
            {
                return NotFound();
            }

            ViewBag.QuizId = id;
            ViewBag.QuizTitle = quiz.Title;
            ViewBag.QuestionCount = quiz.Questions.Count;

            return View(quiz.Questions.OrderBy(q => q.QuestionOrder).ToList());
        }

        // POST: CustomQuiz/AddQuestion
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddQuestion(int quizId, [Bind("QuestionText,Option1,Option2,Option3,Option4,CorrectAnswer")] UserCustomQuizQuestion question)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Index", "Users");
            }

            var quiz = await _context.UserCustomQuizzes
                .Include(q => q.Questions)
                .FirstOrDefaultAsync(q => q.UserQuizId == quizId && q.CreatedByUserId == userId);

            if (quiz == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                question.UserQuizId = quizId;
                question.QuestionOrder = quiz.Questions.Count + 1;

                _context.UserCustomQuizQuestions.Add(question);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Question added successfully!";
                return RedirectToAction("AddQuestions", new { id = quizId });
            }

            TempData["ErrorMessage"] = "Please fill in all fields correctly.";
            return RedirectToAction("AddQuestions", new { id = quizId });
        }

        // POST: CustomQuiz/DeleteQuestion/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteQuestion(int id, int quizId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Index", "Users");
            }

            var question = await _context.UserCustomQuizQuestions
                .Include(q => q.UserQuiz)
                .FirstOrDefaultAsync(q => q.QuestionId == id && q.UserQuiz!.CreatedByUserId == userId);

            if (question == null)
            {
                return NotFound();
            }

            _context.UserCustomQuizQuestions.Remove(question);
            await _context.SaveChangesAsync();

            // Reorder remaining questions
            var remainingQuestions = await _context.UserCustomQuizQuestions
                .Where(q => q.UserQuizId == quizId)
                .OrderBy(q => q.QuestionOrder)
                .ToListAsync();

            int order = 1;
            foreach (var q in remainingQuestions)
            {
                q.QuestionOrder = order++;
            }
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Question deleted successfully!";
            return RedirectToAction("AddQuestions", new { id = quizId });
        }

        // GET: CustomQuiz/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userId == null)
            {
                return RedirectToAction("Index", "Users");
            }

            var quiz = await _context.UserCustomQuizzes
                .Include(q => q.CreatedBy)
                .Include(q => q.Questions.OrderBy(x => x.QuestionOrder))
                .Include(q => q.Assignments)
                    .ThenInclude(a => a.AssignedToUser)
                .FirstOrDefaultAsync(q => q.UserQuizId == id);

            if (quiz == null)
            {
                return NotFound();
            }

            // Check authorization
            bool isCreator = quiz.CreatedByUserId == userId;
            bool isAdmin = userRole == "Admin";
            bool isAssignedUser = quiz.Assignments.Any(a => a.AssignedToUserId == userId);

            if (!isCreator && !isAdmin && !isAssignedUser && !quiz.IsPublic)
            {
                return Forbid();
            }

            ViewBag.CanEdit = isCreator || isAdmin;
            return View(quiz);
        }

        // GET: CustomQuiz/AssignUsers/5
        public async Task<IActionResult> AssignUsers(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Index", "Users");
            }

            var quiz = await _context.UserCustomQuizzes
                .Include(q => q.Questions)
                .Include(q => q.Assignments)
                .FirstOrDefaultAsync(q => q.UserQuizId == id && q.CreatedByUserId == userId);

            if (quiz == null)
            {
                return NotFound();
            }

            if (quiz.Questions.Count == 0)
            {
                TempData["ErrorMessage"] = "Please add at least one question before assigning the quiz.";
                return RedirectToAction("AddQuestions", new { id });
            }

            var users = await _context.Users
                .Where(u => u.Id != userId)
                .OrderBy(u => u.Username)
                .ToListAsync();

            ViewBag.QuizId = id;
            ViewBag.QuizTitle = quiz.Title;
            ViewBag.AssignedUserIds = quiz.Assignments.Select(a => a.AssignedToUserId).ToList();
            return View(users);
        }

        // POST: CustomQuiz/AssignUsers
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignUsers(int quizId, int[] selectedUserIds)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Index", "Users");
            }

            var quiz = await _context.UserCustomQuizzes
                .Include(q => q.Questions)
                .Include(q => q.Assignments)
                .FirstOrDefaultAsync(q => q.UserQuizId == quizId && q.CreatedByUserId == userId);

            if (quiz == null)
            {
                return NotFound();
            }

            if (selectedUserIds != null && selectedUserIds.Length > 0)
            {
                foreach (var assignedUserId in selectedUserIds)
                {
                    // Check if already assigned
                    var existingAssignment = quiz.Assignments
                        .FirstOrDefault(a => a.AssignedToUserId == assignedUserId);

                    if (existingAssignment == null)
                    {
                        var assignment = new UserCustomQuizAssignment
                        {
                            UserQuizId = quizId,
                            AssignedToUserId = assignedUserId,
                            AssignedDate = DateTime.Now,
                            IsCompleted = false,
                            IsViewed = false,
                            TotalQuestions = quiz.Questions.Count
                        };
                        _context.UserCustomQuizAssignments.Add(assignment);
                    }
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Quiz assigned to {selectedUserIds.Length} user(s) successfully!";
            }

            return RedirectToAction("MyQuizzes");
        }

        // GET: CustomQuiz/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userId == null)
            {
                return RedirectToAction("Index", "Users");
            }

            var quiz = await _context.UserCustomQuizzes
                .Include(q => q.CreatedBy)
                .Include(q => q.Questions)
                .FirstOrDefaultAsync(q => q.UserQuizId == id);

            if (quiz == null)
            {
                return NotFound();
            }

            if (quiz.CreatedByUserId != userId && userRole != "Admin")
            {
                return Forbid();
            }

            return View(quiz);
        }

        // POST: CustomQuiz/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userId == null)
            {
                return RedirectToAction("Index", "Users");
            }

            var quiz = await _context.UserCustomQuizzes
                .FirstOrDefaultAsync(q => q.UserQuizId == id);

            if (quiz == null)
            {
                return NotFound();
            }

            if (quiz.CreatedByUserId != userId && userRole != "Admin")
            {
                return Forbid();
            }

            // Soft delete
            quiz.IsActive = false;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Quiz deleted successfully!";
            return RedirectToAction("MyQuizzes");
        }
    }
}
