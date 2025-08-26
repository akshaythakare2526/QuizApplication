using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuizApplication.Data;
using QuizApplication.ViewModels;
using System.Text.Json;

namespace QuizApplication.Controllers
{
    public class PracticeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PracticeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Check if user is authenticated
        private bool IsUserAuthenticated()
        {
            return HttpContext.Session.GetInt32("UserId") != null;
        }

        // Redirect to login if not authenticated
        private IActionResult? RedirectIfNotAuthenticated()
        {
            if (!IsUserAuthenticated())
            {
                return RedirectToAction("Index", "Users");
            }
            return null;
        }

        // GET: Practice/Setup
        public async Task<IActionResult> Setup(string mode = "")
        {
            var redirectResult = RedirectIfNotAuthenticated();
            if (redirectResult != null) return redirectResult;

            var model = new PracticeSetupViewModel
            {
                PracticeMode = mode
            };

            // Get available categories with question counts
            var categories = await _context.Categories
                .Select(c => new CategoryOption
                {
                    CategoryId = c.CategoryId,
                    CategoryName = c.CategoryName,
                    QuestionCount = _context.Questions.Count(q => q.CategoryId == c.CategoryId),
                    IsSelected = false
                })
                .Where(c => c.QuestionCount > 0)
                .ToListAsync();

            model.AvailableCategories = categories;

            return View(model);
        }

        // POST: Practice/Setup
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Setup(PracticeSetupViewModel model)
        {
            var redirectResult = RedirectIfNotAuthenticated();
            if (redirectResult != null) return redirectResult;

            if (!ModelState.IsValid)
            {
                // Reload categories if validation fails
                var categories = await _context.Categories
                    .Select(c => new CategoryOption
                    {
                        CategoryId = c.CategoryId,
                        CategoryName = c.CategoryName,
                        QuestionCount = _context.Questions.Count(q => q.CategoryId == c.CategoryId),
                        IsSelected = model.SelectedCategoryIds.Contains(c.CategoryId)
                    })
                    .Where(c => c.QuestionCount > 0)
                    .ToListAsync();

                model.AvailableCategories = categories;
                return View(model);
            }

            // Store practice settings in session
            HttpContext.Session.SetString("PracticeMode", model.PracticeMode);
            HttpContext.Session.SetString("PracticeDifficulty", model.DifficultyLevel ?? "");
            HttpContext.Session.SetString("PracticeCategories", JsonSerializer.Serialize(model.SelectedCategoryIds));

            return RedirectToAction("Question");
        }

        // GET: Practice/Question
        public async Task<IActionResult> Question()
        {
            var redirectResult = RedirectIfNotAuthenticated();
            if (redirectResult != null) return redirectResult;

            // Get practice settings from session
            var practiceMode = HttpContext.Session.GetString("PracticeMode") ?? "";
            var difficulty = HttpContext.Session.GetString("PracticeDifficulty") ?? "";
            var categoriesJson = HttpContext.Session.GetString("PracticeCategories") ?? "[]";
            var categoryIds = JsonSerializer.Deserialize<List<int>>(categoriesJson) ?? new List<int>();

            // Build query based on practice mode
            var questionsQuery = _context.Questions.Include(q => q.Category).AsQueryable();

            if (practiceMode == "difficulty" && !string.IsNullOrEmpty(difficulty))
            {
                questionsQuery = questionsQuery.Where(q => q.DifficultyLevel == difficulty);
            }
            else if (practiceMode == "category" && categoryIds.Any())
            {
                questionsQuery = questionsQuery.Where(q => categoryIds.Contains(q.CategoryId));
                if (!string.IsNullOrEmpty(difficulty))
                {
                    questionsQuery = questionsQuery.Where(q => q.DifficultyLevel == difficulty);
                }
            }

            // Get a random question
            var questions = await questionsQuery.ToListAsync();
            if (!questions.Any())
            {
                TempData["Error"] = "No questions found for the selected criteria.";
                return RedirectToAction("Setup");
            }

            var random = new Random();
            var selectedQuestion = questions[random.Next(questions.Count)];

            var model = new PracticeQuestionViewModel
            {
                QuestionId = selectedQuestion.QuestionId,
                QuestionText = selectedQuestion.QuestionText,
                QuestionImageData = selectedQuestion.QuestionImageData,
                Option1 = selectedQuestion.Option1,
                Option2 = selectedQuestion.Option2,
                Option3 = selectedQuestion.Option3,
                Option4 = selectedQuestion.Option4,
                CorrectAnswer = selectedQuestion.CorrectedOption,
                DifficultyLevel = selectedQuestion.DifficultyLevel,
                CategoryName = selectedQuestion.Category?.CategoryName ?? "",
                ShowAnswer = false
            };

            return View(model);
        }

        // POST: Practice/CheckAnswer
        [HttpPost]
        public async Task<IActionResult> CheckAnswer(int questionId, string selectedOption)
        {
            var redirectResult = RedirectIfNotAuthenticated();
            if (redirectResult != null) return Json(new { success = false, message = "Not authenticated" });

            var question = await _context.Questions
                .Include(q => q.Category)
                .FirstOrDefaultAsync(q => q.QuestionId == questionId);

            if (question == null)
            {
                return Json(new { success = false, message = "Question not found" });
            }

            var isCorrect = question.CorrectedOption == selectedOption;
            var correctAnswerText = "";

            switch (question.CorrectedOption)
            {
                case "Option1": correctAnswerText = question.Option1; break;
                case "Option2": correctAnswerText = question.Option2; break;
                case "Option3": correctAnswerText = question.Option3; break;
                case "Option4": correctAnswerText = question.Option4; break;
            }

            return Json(new
            {
                success = true,
                isCorrect = isCorrect,
                correctAnswer = question.CorrectedOption,
                correctAnswerText = correctAnswerText,
                explanation = isCorrect ? "Correct! Well done!" : $"Incorrect. The correct answer is {correctAnswerText}."
            });
        }

        // GET: Practice/GetImage
        public async Task<IActionResult> GetImage(int questionId)
        {
            var question = await _context.Questions.FindAsync(questionId);
            if (question?.QuestionImageData != null)
            {
                return File(question.QuestionImageData, question.ImageContentType ?? "image/jpeg");
            }
            return NotFound();
        }

        // GET: Practice/NextQuestion
        public IActionResult NextQuestion()
        {
            return RedirectToAction("Question");
        }
    }
}
