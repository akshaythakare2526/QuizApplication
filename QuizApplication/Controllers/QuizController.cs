using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuizApplication.Data;
using QuizApplication.Models;
using QuizApplication.ViewModels;
using System.Text.Json;

namespace QuizApplication.Controllers
{
    public class QuizController : Controller
    {
        private readonly ApplicationDbContext _context;

        public QuizController(ApplicationDbContext context)
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

        // GET: Quiz/Setup
        public async Task<IActionResult> Setup()
        {
            var redirectResult = RedirectIfNotAuthenticated();
            if (redirectResult != null) return redirectResult;

            var model = new QuizSetupViewModel();
            
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

        // POST: Quiz/Setup
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Setup(QuizSetupViewModel model)
        {
            var redirectResult = RedirectIfNotAuthenticated();
            if (redirectResult != null) return redirectResult;

            if (!model.SelectedCategoryIds.Any())
            {
                ModelState.AddModelError("SelectedCategoryIds", "Please select at least one category.");
            }

            if (ModelState.IsValid)
            {
                var userId = HttpContext.Session.GetInt32("UserId")!.Value;

                // Create quiz session
                var quizSession = new QuizSessions
                {
                    UserId = userId,
                    StartTime = DateTime.Now,
                    IsCompleted = false,
                    TimeLimit = model.TimeLimit,
                    NumberOfQuestions = model.NumberOfQuestions,
                    DifficultyLevel = model.DifficultyLevel,
                    SelectedCategories = JsonSerializer.Serialize(model.SelectedCategoryIds),
                    QuizTitle = model.QuizTitle,
                    TotalScore = 0,
                    MaxPossibleScore = model.NumberOfQuestions
                };

                _context.QuizSessions.Add(quizSession);
                await _context.SaveChangesAsync();

                return RedirectToAction("Start", new { sessionId = quizSession.SessionId });
            }

            // Repopulate categories if validation fails
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

        // GET: Quiz/Start/5
        public async Task<IActionResult> Start(int sessionId)
        {
            var redirectResult = RedirectIfNotAuthenticated();
            if (redirectResult != null) return redirectResult;

            var userId = HttpContext.Session.GetInt32("UserId")!.Value;
            var session = await _context.QuizSessions
                .FirstOrDefaultAsync(s => s.SessionId == sessionId && s.UserId == userId);

            if (session == null || session.IsCompleted)
            {
                return NotFound();
            }

            return View(session);
        }

        // GET: Quiz/Question/5
        public async Task<IActionResult> Question(int sessionId, int questionIndex = 0)
        {
            var redirectResult = RedirectIfNotAuthenticated();
            if (redirectResult != null) return redirectResult;

            var userId = HttpContext.Session.GetInt32("UserId")!.Value;
            var session = await _context.QuizSessions
                .FirstOrDefaultAsync(s => s.SessionId == sessionId && s.UserId == userId);

            if (session == null || session.IsCompleted)
            {
                return NotFound();
            }

            // Check if session has expired
            var timeElapsed = (DateTime.Now - session.StartTime).TotalMinutes;
            if (timeElapsed > session.TimeLimit)
            {
                await CompleteSession(sessionId);
                return RedirectToAction("Result", new { sessionId });
            }

            // Get questions based on session criteria
            var categoryIds = JsonSerializer.Deserialize<List<int>>(session.SelectedCategories ?? "[]") ?? new List<int>();
            var questionsQuery = _context.Questions
                .Include(q => q.Category)
                .Where(q => categoryIds.Contains(q.CategoryId));

            if (!string.IsNullOrEmpty(session.DifficultyLevel))
            {
                questionsQuery = questionsQuery.Where(q => q.DifficultyLevel == session.DifficultyLevel);
            }

            var questions = await questionsQuery
                .OrderBy(q => Guid.NewGuid()) // Random order
                .Take(session.NumberOfQuestions)
                .ToListAsync();

            if (questionIndex >= questions.Count)
            {
                await CompleteSession(sessionId);
                return RedirectToAction("Result", new { sessionId });
            }

            var currentQuestion = questions[questionIndex];
            
            // Get user's previous answers
            var userAnswers = await _context.UserAnswers
                .Where(ua => ua.SessionId == sessionId)
                .ToListAsync();

            var answeredQuestionIds = userAnswers.Select(ua => ua.QuestionId).ToList();
            var allQuestionIds = questions.Select(q => q.QuestionId).ToList();

            var userAnswer = userAnswers.FirstOrDefault(ua => ua.QuestionId == currentQuestion.QuestionId);

            var remainingTime = (int)((session.TimeLimit * 60) - (DateTime.Now - session.StartTime).TotalSeconds);

            var model = new QuizQuestionViewModel
            {
                SessionId = sessionId,
                QuestionId = currentQuestion.QuestionId,
                CurrentQuestionIndex = questionIndex,
                TotalQuestions = questions.Count,
                QuestionText = currentQuestion.QuestionText,
                QuestionImageData = currentQuestion.QuestionImageData,
                ImageContentType = currentQuestion.ImageContentType,
                Option1 = currentQuestion.Option1,
                Option2 = currentQuestion.Option2,
                Option3 = currentQuestion.Option3,
                Option4 = currentQuestion.Option4,
                SelectedOption = userAnswer?.SelectedOption,
                DifficultyLevel = currentQuestion.DifficultyLevel,
                CategoryName = currentQuestion.Category?.CategoryName ?? "",
                RemainingTimeInSeconds = Math.Max(0, remainingTime),
                HasPrevious = questionIndex > 0,
                HasNext = questionIndex < questions.Count - 1,
                AnsweredQuestions = answeredQuestionIds,
                AllQuestionIds = allQuestionIds
            };

            return View(model);
        }

        // POST: Quiz/SaveAnswer
        [HttpPost]
        public async Task<IActionResult> SaveAnswer(int sessionId, int questionId, string selectedOption, int timeTaken)
        {
            var redirectResult = RedirectIfNotAuthenticated();
            if (redirectResult != null) return Json(new { success = false, message = "Not authenticated" });

            var userId = HttpContext.Session.GetInt32("UserId")!.Value;
            var session = await _context.QuizSessions
                .FirstOrDefaultAsync(s => s.SessionId == sessionId && s.UserId == userId);

            if (session == null || session.IsCompleted)
            {
                return Json(new { success = false, message = "Invalid session" });
            }

            var question = await _context.Questions.FindAsync(questionId);
            if (question == null)
            {
                return Json(new { success = false, message = "Question not found" });
            }

            // Check if answer already exists
            var existingAnswer = await _context.UserAnswers
                .FirstOrDefaultAsync(ua => ua.SessionId == sessionId && ua.QuestionId == questionId);

            var isCorrect = selectedOption == question.CorrectedOption;

            if (existingAnswer != null)
            {
                // Update existing answer
                existingAnswer.SelectedOption = selectedOption;
                existingAnswer.IsCorrect = isCorrect;
                existingAnswer.AnsweredAt = DateTime.Now;
                existingAnswer.TimeTaken = timeTaken;
                _context.Update(existingAnswer);
            }
            else
            {
                // Create new answer
                var userAnswer = new UserAnswers
                {
                    SessionId = sessionId,
                    QuestionId = questionId,
                    SelectedOption = selectedOption,
                    IsCorrect = isCorrect,
                    AnsweredAt = DateTime.Now,
                    TimeTaken = timeTaken
                };
                _context.UserAnswers.Add(userAnswer);
            }

            await _context.SaveChangesAsync();

            return Json(new { success = true, isCorrect = isCorrect });
        }

        // POST: Quiz/CompleteQuiz
        [HttpPost]
        public async Task<IActionResult> CompleteQuiz(int sessionId)
        {
            var redirectResult = RedirectIfNotAuthenticated();
            if (redirectResult != null) return Json(new { success = false, message = "Not authenticated" });

            await CompleteSession(sessionId);
            return Json(new { success = true });
        }

        // GET: Quiz/Result/5
        public async Task<IActionResult> Result(int sessionId)
        {
            var redirectResult = RedirectIfNotAuthenticated();
            if (redirectResult != null) return redirectResult;

            var userId = HttpContext.Session.GetInt32("UserId")!.Value;
            var session = await _context.QuizSessions
                .FirstOrDefaultAsync(s => s.SessionId == sessionId && s.UserId == userId);

            if (session == null)
            {
                return NotFound();
            }

            var userAnswers = await _context.UserAnswers
                .Include(ua => ua.Question!)
                .ThenInclude(q => q.Category!)
                .Where(ua => ua.SessionId == sessionId)
                .ToListAsync();

            var correctAnswers = userAnswers.Count(ua => ua.IsCorrect);
            var wrongAnswers = userAnswers.Count(ua => !ua.IsCorrect);
            var totalQuestions = session.NumberOfQuestions;
            var unansweredQuestions = totalQuestions - userAnswers.Count;

            var questionResults = userAnswers.Select(ua => new QuestionResultViewModel
            {
                QuestionId = ua.QuestionId,
                QuestionText = ua.Question?.QuestionText ?? "",
                CategoryName = ua.Question?.Category?.CategoryName ?? "",
                DifficultyLevel = ua.Question?.DifficultyLevel ?? "",
                CorrectAnswer = ua.Question?.CorrectedOption ?? "",
                UserAnswer = ua.SelectedOption,
                IsCorrect = ua.IsCorrect,
                TimeTaken = ua.TimeTaken,
                Option1 = ua.Question?.Option1 ?? "",
                Option2 = ua.Question?.Option2 ?? "",
                Option3 = ua.Question?.Option3 ?? "",
                Option4 = ua.Question?.Option4 ?? ""
            }).ToList();

            var model = new QuizResultViewModel
            {
                SessionId = sessionId,
                QuizTitle = session.QuizTitle ?? "Quiz",
                TotalQuestions = totalQuestions,
                CorrectAnswers = correctAnswers,
                WrongAnswers = wrongAnswers,
                UnansweredQuestions = unansweredQuestions,
                Percentage = totalQuestions > 0 ? (double)correctAnswers / totalQuestions * 100 : 0,
                TimeTaken = session.EndTime.HasValue ? session.EndTime.Value - session.StartTime : TimeSpan.Zero,
                StartTime = session.StartTime,
                EndTime = session.EndTime ?? DateTime.Now,
                QuestionResults = questionResults
            };

            return View(model);
        }

        // GET: Quiz/GetQuestions (AJAX endpoint)
        [HttpGet]
        public async Task<IActionResult> GetQuestions(int sessionId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return Json(new { success = false, message = "Not authenticated" });
            }

            var session = await _context.QuizSessions
                .FirstOrDefaultAsync(s => s.SessionId == sessionId && s.UserId == userId);

            if (session == null)
            {
                return Json(new { success = false, message = "Session not found" });
            }

            // Get or create fixed question order for this session
            var questionOrder = await GetOrCreateQuestionOrder(sessionId, session);

            var questions = await _context.Questions
                .Include(q => q.Category)
                .Where(q => questionOrder.Contains(q.QuestionId))
                .Select(q => new
                {
                    questionId = q.QuestionId,
                    questionText = q.QuestionText,
                    hasImage = q.QuestionImageData != null,
                    option1 = q.Option1,
                    option2 = q.Option2,
                    option3 = q.Option3,
                    option4 = q.Option4,
                    correctAnswer = q.CorrectedOption,
                    difficulty = q.DifficultyLevel,
                    category = q.Category!.CategoryName
                })
                .ToListAsync();

            // Order questions according to the fixed sequence
            var orderedQuestions = questionOrder
                .Select(id => questions.FirstOrDefault(q => q.questionId == id))
                .Where(q => q != null)
                .ToList();

            return Json(new { success = true, questions = orderedQuestions });
        }

        // GET: Quiz/GetQuestion (AJAX endpoint for single question)
        [HttpGet]
        public async Task<IActionResult> GetQuestion(int sessionId, int questionIndex)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return Json(new { success = false, message = "Not authenticated" });
            }

            var session = await _context.QuizSessions
                .FirstOrDefaultAsync(s => s.SessionId == sessionId && s.UserId == userId);

            if (session == null || session.IsCompleted)
            {
                return Json(new { success = false, message = "Session not found or completed" });
            }

            // Check if session has expired
            var timeElapsed = (DateTime.Now - session.StartTime).TotalMinutes;
            if (timeElapsed > session.TimeLimit)
            {
                await CompleteSession(sessionId);
                return Json(new { success = false, message = "Time expired", expired = true });
            }

            // Get fixed question order for this session
            var questionOrder = await GetOrCreateQuestionOrder(sessionId, session);

            if (questionIndex >= questionOrder.Count)
            {
                await CompleteSession(sessionId);
                return Json(new { success = false, message = "Quiz completed", completed = true });
            }

            var questionId = questionOrder[questionIndex];
            var currentQuestion = await _context.Questions
                .Include(q => q.Category)
                .FirstOrDefaultAsync(q => q.QuestionId == questionId);

            if (currentQuestion == null)
            {
                return Json(new { success = false, message = "Question not found" });
            }

            // Get user's previous answers
            var userAnswers = await _context.UserAnswers
                .Where(ua => ua.SessionId == sessionId)
                .ToListAsync();

            var answeredQuestionIds = userAnswers.Select(ua => ua.QuestionId).ToList();
            var userAnswer = userAnswers.FirstOrDefault(ua => ua.QuestionId == currentQuestion.QuestionId);

            var remainingTime = (int)((session.TimeLimit * 60) - (DateTime.Now - session.StartTime).TotalSeconds);

            var questionData = new
            {
                success = true,
                sessionId = sessionId,
                questionId = currentQuestion.QuestionId,
                currentQuestionIndex = questionIndex,
                totalQuestions = questionOrder.Count,
                questionText = currentQuestion.QuestionText,
                hasImage = currentQuestion.QuestionImageData != null,
                imageUrl = currentQuestion.QuestionImageData != null ? Url.Action("GetImage", "Quiz", new { questionId = currentQuestion.QuestionId }) : null,
                option1 = currentQuestion.Option1,
                option2 = currentQuestion.Option2,
                option3 = currentQuestion.Option3,
                option4 = currentQuestion.Option4,
                selectedOption = userAnswer?.SelectedOption,
                difficultyLevel = currentQuestion.DifficultyLevel,
                categoryName = currentQuestion.Category?.CategoryName ?? "",
                remainingTimeInSeconds = Math.Max(0, remainingTime),
                hasPrevious = questionIndex > 0,
                hasNext = questionIndex < questionOrder.Count - 1,
                answeredQuestions = answeredQuestionIds,
                allQuestionIds = questionOrder
            };

            return Json(questionData);
        }

        private async Task CompleteSession(int sessionId)
        {
            var session = await _context.QuizSessions.FindAsync(sessionId);
            if (session != null && !session.IsCompleted)
            {
                session.IsCompleted = true;
                session.EndTime = DateTime.Now;

                // Calculate final score
                var correctAnswers = await _context.UserAnswers
                    .CountAsync(ua => ua.SessionId == sessionId && ua.IsCorrect);
                session.TotalScore = correctAnswers;

                _context.Update(session);
                await _context.SaveChangesAsync();
            }
        }

        private async Task<List<int>> GetOrCreateQuestionOrder(int sessionId, QuizSessions session)
        {
            // Check if we have stored the question order in session (you could also store in database)
            var questionOrderKey = $"QuestionOrder_{sessionId}";
            
            // For now, let's store it in a simple way - you might want to store this in database for persistence
            if (HttpContext.Session.GetString(questionOrderKey) != null)
            {
                var storedOrder = JsonSerializer.Deserialize<List<int>>(HttpContext.Session.GetString(questionOrderKey)!);
                return storedOrder ?? new List<int>();
            }

            // Create new question order
            var categoryIds = JsonSerializer.Deserialize<List<int>>(session.SelectedCategories ?? "[]") ?? new List<int>();
            var questionsQuery = _context.Questions.Where(q => categoryIds.Contains(q.CategoryId));

            if (!string.IsNullOrEmpty(session.DifficultyLevel))
            {
                questionsQuery = questionsQuery.Where(q => q.DifficultyLevel == session.DifficultyLevel);
            }

            var questionIds = await questionsQuery
                .OrderBy(q => Guid.NewGuid()) // Random order, but fixed once created
                .Take(session.NumberOfQuestions)
                .Select(q => q.QuestionId)
                .ToListAsync();

            // Store the order in session
            HttpContext.Session.SetString(questionOrderKey, JsonSerializer.Serialize(questionIds));
            return questionIds;
        }

        // GET: Quiz/GetImage/5
        public async Task<IActionResult> GetImage(int questionId)
        {
            var question = await _context.Questions.FindAsync(questionId);
            if (question?.QuestionImageData == null)
            {
                return NotFound();
            }

            return File(question.QuestionImageData, question.ImageContentType ?? "image/jpeg");
        }
    }
}
