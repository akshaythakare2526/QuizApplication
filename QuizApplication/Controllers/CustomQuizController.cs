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

        // GET: CustomQuiz/SelectQuestions/5 -> Redirect to AddQuestions for now
        public IActionResult SelectQuestions(int id)
        {
            return RedirectToAction("AddQuestions", new { id });
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
                .Include(q => q.Questions)
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

            var vm = new CustomQuizDetailsViewModel
            {
                CustomQuizId = quiz.UserQuizId,
                Title = quiz.Title,
                Description = quiz.Description,
                CreatedBy = quiz.CreatedBy?.Username ?? "user",
                CreatedDate = quiz.CreatedDate,
                TimeLimit = quiz.TimeLimit,
                IsPublic = quiz.IsPublic,
                CategoryName = null,
                DifficultyLevel = null,
                QuestionCount = quiz.Questions?.Count ?? 0,
                AssignedUsers = quiz.Assignments.Select(a => new AssignedUserInfo
                {
                    Username = a.AssignedToUser!.Username,
                    AssignedDate = a.AssignedDate,
                    IsCompleted = a.IsCompleted,
                    CompletedDate = a.CompletedDate,
                    Score = a.Score
                }).ToList(),
                CanEdit = isCreator || isAdmin,
                CanTake = quiz.IsPublic || isAssignedUser || isCreator || isAdmin
            };

            return View(vm);
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

            var vm = new AssignQuizViewModel
            {
                CustomQuizId = id,
                QuizTitle = quiz.Title,
                AvailableUsers = users.Select(u => new UserSelectionItem
                {
                    UserId = u.Id,
                    Username = u.Username,
                    Email = u.Email,
                    IsSelected = quiz.Assignments.Any(a => a.AssignedToUserId == u.Id)
                }).ToList()
            };

            return View(vm);
        }

        // POST: CustomQuiz/AssignUsers
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignUsers(int customQuizId, int[] selectedUserIds)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Index", "Users");
            }

            var quiz = await _context.UserCustomQuizzes
                .Include(q => q.Questions)
                .Include(q => q.Assignments)
                .FirstOrDefaultAsync(q => q.UserQuizId == customQuizId && q.CreatedByUserId == userId);

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
                            UserQuizId = customQuizId,
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

        // GET: CustomQuiz/Take/5 - Start taking a custom quiz
        public async Task<IActionResult> Take(int id)
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
                .Include(q => q.Assignments)
                .FirstOrDefaultAsync(q => q.UserQuizId == id && q.IsActive);

            if (quiz == null)
            {
                return NotFound();
            }

            bool isCreator = quiz.CreatedByUserId == userId;
            bool isAdmin = userRole == "Admin";
            bool isAssignedUser = quiz.Assignments.Any(a => a.AssignedToUserId == userId);
            if (!isCreator && !isAdmin && !isAssignedUser && !quiz.IsPublic)
            {
                return Forbid();
            }

            var vm = new TakeCustomQuizViewModel
            {
                CustomQuizId = quiz.UserQuizId,
                Title = quiz.Title,
                TimeLimit = quiz.TimeLimit,
                StartTime = DateTime.Now,
                Questions = quiz.Questions
                    .OrderBy(q => q.QuestionOrder)
                    .Select(q => new CustomQuizQuestionItem
                    {
                        QuestionId = q.QuestionId,
                        QuestionText = q.QuestionText,
                        Option1 = q.Option1,
                        Option2 = q.Option2,
                        Option3 = q.Option3,
                        Option4 = q.Option4,
                        QuestionOrder = q.QuestionOrder
                    }).ToList()
            };

            return View(vm);
        }

        // POST: CustomQuiz/Submit - submit custom quiz answers
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(int customQuizId, Dictionary<int, string> answers)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Index", "Users");
            }

            var quiz = await _context.UserCustomQuizzes
                .Include(q => q.Questions)
                .Include(q => q.Assignments)
                .FirstOrDefaultAsync(q => q.UserQuizId == customQuizId && q.IsActive);

            if (quiz == null)
            {
                return NotFound();
            }

            // Find or create assignment for this user
            var assignment = quiz.Assignments.FirstOrDefault(a => a.AssignedToUserId == userId);
            if (assignment == null)
            {
                assignment = new UserCustomQuizAssignment
                {
                    UserQuizId = customQuizId,
                    AssignedToUserId = userId.Value,
                    AssignedDate = DateTime.Now,
                    IsCompleted = false,
                    IsViewed = true,
                    TotalQuestions = quiz.Questions.Count
                };
                _context.UserCustomQuizAssignments.Add(assignment);
                await _context.SaveChangesAsync();
            }

            // Remove any previous answers for this assignment
            var previousAnswers = _context.UserCustomQuizAnswers.Where(a => a.AssignmentId == assignment.AssignmentId);
            _context.UserCustomQuizAnswers.RemoveRange(previousAnswers);

            int correct = 0;
            foreach (var q in quiz.Questions)
            {
                answers.TryGetValue(q.QuestionId, out var selected);
                bool isCorrect = !string.IsNullOrEmpty(selected) && selected == q.CorrectAnswer;

                var ans = new UserCustomQuizAnswer
                {
                    AssignmentId = assignment.AssignmentId,
                    QuestionId = q.QuestionId,
                    SelectedAnswer = selected ?? string.Empty,
                    IsCorrect = isCorrect,
                    AnsweredDate = DateTime.Now
                };
                _context.UserCustomQuizAnswers.Add(ans);
                if (isCorrect) correct++;
            }

            // finalize assignment
            assignment.IsCompleted = true;
            assignment.CompletedDate = DateTime.Now;
            assignment.Score = correct;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"You scored {correct} out of {quiz.Questions.Count}.";
            return RedirectToAction("Details", new { id = customQuizId });
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

            var vm = new CustomQuizDetailsViewModel
            {
                CustomQuizId = quiz.UserQuizId,
                Title = quiz.Title,
                Description = quiz.Description,
                CreatedBy = quiz.CreatedBy?.Username ?? "user",
                CreatedDate = quiz.CreatedDate,
                TimeLimit = quiz.TimeLimit,
                IsPublic = quiz.IsPublic,
                CategoryName = null,
                DifficultyLevel = null,
                QuestionCount = quiz.Questions?.Count ?? 0
            };

            return View(vm);
        }

        // POST: CustomQuiz/Delete
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePost(int customQuizId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userId == null)
            {
                return RedirectToAction("Index", "Users");
            }

            var quiz = await _context.UserCustomQuizzes
                .FirstOrDefaultAsync(q => q.UserQuizId == customQuizId);

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
