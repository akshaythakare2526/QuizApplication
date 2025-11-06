using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuizApplication.Data;
using QuizApplication.Models;
using System.Security.Cryptography;
using System.Text;

namespace QuizApplication.Controllers
{
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UsersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Users (Login Page)
        public IActionResult Index()
        {
            return View();
        }

        // POST: Users/Login
        [HttpPost]
        public async Task<IActionResult> Login(string UserNameOrEmail, string password)
        {
            if (string.IsNullOrEmpty(UserNameOrEmail) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Email and password are required.";
                return View("Index");
            }

            var hashedPassword = HashPassword(password);
            var user = await _context.Users.FirstOrDefaultAsync(u => (u.Email == UserNameOrEmail || u.Username == UserNameOrEmail) && u.PasswordHash == hashedPassword);

            if (user != null)
            {
                // Store user info in session
                HttpContext.Session.SetInt32("UserId", user.Id);
                HttpContext.Session.SetString("Username", user.Username);
                HttpContext.Session.SetString("UserRole", user.Role);

                return RedirectToAction("Dashboard");
            }

            ViewBag.Error = "Invalid UserNameOrEmail or password.";
            return View("Index");
        }

        // GET: Users/Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: Users/Register
        [HttpPost]
        public async Task<IActionResult> Register(Users user, string confirmPassword)
        {
            if (user.Username == null || user.Email == null || user.PasswordHash == null || confirmPassword == null)
            {
                ViewBag.Error = "All fields are required.";
                return View(user);
            }

            if (user.PasswordHash != confirmPassword)
            {
                ViewBag.Error = "Passwords do not match.";
                return View(user);
            }

            // Check if user already exists
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == user.Email);
            if (existingUser != null)
            {
                ViewBag.Error = "User with this UserNameOrEmail already exists.";
                return View(user);
            }

            // Hash the password
            user.PasswordHash = HashPassword(user.PasswordHash);
            
            // Set default role if not provided
            if (string.IsNullOrEmpty(user.Role))
            {
                user.Role = "User";
            }

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            ViewBag.Success = "Registration successful! Please login.";
            return View("Index");
        }

        // GET: Users/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Index");
            }

            var username = HttpContext.Session.GetString("Username");
            var userRole = HttpContext.Session.GetString("UserRole");
            
            ViewBag.Username = username;
            ViewBag.UserRole = userRole;
            ViewBag.UserId = userId;

            // Get current user details
            var currentUser = await _context.Users.FindAsync(userId);
            ViewBag.CurrentUser = currentUser;

            // Get statistics for dashboard
            if (userRole == "Admin")
            {
                // Admin dashboard data
                ViewBag.TotalUsers = await _context.Users.CountAsync();
                ViewBag.AdminUsers = await _context.Users.CountAsync(u => u.Role == "Admin");
                ViewBag.RegularUsers = await _context.Users.CountAsync(u => u.Role == "User");
                ViewBag.TotalCategories = await _context.Categories.CountAsync();
                ViewBag.TotalQuestions = await _context.Questions.CountAsync();
                ViewBag.TotalQuizSessions = await _context.QuizSessions.CountAsync();
                
                return View("AdminDashboard");
            }
            else
            {
                // User dashboard data
                ViewBag.MyQuizSessions = await _context.QuizSessions.CountAsync(qs => qs.UserId == userId);
                ViewBag.CompletedQuizzes = await _context.QuizSessions.CountAsync(qs => qs.UserId == userId && qs.IsCompleted);
                
                // Get user's answers through quiz sessions
                var userSessionIds = await _context.QuizSessions
                    .Where(qs => qs.UserId == userId)
                    .Select(qs => qs.SessionId)
                    .ToListAsync();
                ViewBag.MyAnswers = await _context.UserAnswers.CountAsync(ua => userSessionIds.Contains(ua.SessionId));
                ViewBag.AvailableCategories = await _context.Categories.CountAsync();
                
                // Custom Quiz Notifications
                var newCustomQuizzes = await _context.UserCustomQuizAssignments
                    .Include(a => a.UserQuiz)
                    .ThenInclude(q => q!.CreatedBy)
                    .Include(a => a.UserQuiz)
                    .ThenInclude(q => q!.Questions)
                    .Where(a => a.AssignedToUserId == userId && !a.IsViewed)
                    .OrderByDescending(a => a.AssignedDate)
                    .Take(5)
                    .ToListAsync();
                ViewBag.NewCustomQuizzes = newCustomQuizzes;
                ViewBag.NewCustomQuizCount = newCustomQuizzes.Count;
                // Mark fetched notifications as viewed so they don't keep showing repeatedly
                if (newCustomQuizzes.Count > 0)
                {
                    foreach (var a in newCustomQuizzes)
                    {
                        a.IsViewed = true;
                    }
                    await _context.SaveChangesAsync();
                }
                
                // Total custom quizzes assigned
                ViewBag.TotalAssignedQuizzes = await _context.UserCustomQuizAssignments
                    .CountAsync(a => a.AssignedToUserId == userId);
                ViewBag.PendingCustomQuizzes = await _context.UserCustomQuizAssignments
                    .CountAsync(a => a.AssignedToUserId == userId && !a.IsCompleted);
                
                // Fetch a small list of pending assignments to show in a banner (top 5)
                var pendingAssignments = await _context.UserCustomQuizAssignments
                    .Include(a => a.UserQuiz)
                        .ThenInclude(q => q!.CreatedBy)
                    .Include(a => a.UserQuiz)
                        .ThenInclude(q => q!.Questions)
                    .Where(a => a.AssignedToUserId == userId && !a.IsCompleted)
                    .OrderByDescending(a => a.AssignedDate)
                    .Take(5)
                    .ToListAsync();
                ViewBag.PendingAssignments = pendingAssignments;
                
                return View("UserDashboard");
            }
        }

        // GET: Users/Manage (User CRUD operations)
        public async Task<IActionResult> Manage()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Index");
            }

            var users = await _context.Users.ToListAsync();
            return View(users);
        }

        // GET: Users/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Index");
            }

            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // GET: Users/Create
        public IActionResult Create()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Index");
            }

            return View();
        }

        // POST: Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Username,Email,PasswordHash,Role")] Users user)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Index");
            }

            if (ModelState.IsValid)
            {
                // Hash the password
                user.PasswordHash = HashPassword(user.PasswordHash);
                
                _context.Add(user);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Manage));
            }
            return View(user);
        }

        // GET: Users/EditProfile - User edits their own profile
        public async Task<IActionResult> EditProfile()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Index");
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return RedirectToAction("Logout");
            }

            return View(user);
        }

        // POST: Users/EditProfile - Update user's own profile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile([Bind("Id,Username,Email")] Users user, string currentPassword, string newPassword, string confirmPassword, bool changePassword = false)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null || userId != user.Id)
            {
                return RedirectToAction("Index");
            }

            var existingUser = await _context.Users.FindAsync(userId);
            if (existingUser == null)
            {
                return RedirectToAction("Logout");
            }

            // Validate input
            if (string.IsNullOrEmpty(user.Username) || string.IsNullOrEmpty(user.Email))
            {
                ViewBag.Error = "Username and Email are required.";
                return View(existingUser);
            }

            // Check if email is already used by another user
            var emailExists = await _context.Users.AnyAsync(u => u.Email == user.Email && u.Id != userId);
            if (emailExists)
            {
                ViewBag.Error = "This email is already in use by another user.";
                return View(existingUser);
            }

            // Validate password change if requested
            if (changePassword)
            {
                if (string.IsNullOrEmpty(currentPassword))
                {
                    ViewBag.Error = "Current password is required to change password.";
                    return View(existingUser);
                }

                if (HashPassword(currentPassword) != existingUser.PasswordHash)
                {
                    ViewBag.Error = "Current password is incorrect.";
                    return View(existingUser);
                }

                if (string.IsNullOrEmpty(newPassword))
                {
                    ViewBag.Error = "New password is required.";
                    return View(existingUser);
                }

                if (newPassword != confirmPassword)
                {
                    ViewBag.Error = "New passwords do not match.";
                    return View(existingUser);
                }

                if (newPassword.Length < 6)
                {
                    ViewBag.Error = "Password must be at least 6 characters long.";
                    return View(existingUser);
                }

                existingUser.PasswordHash = HashPassword(newPassword);
            }

            // Update profile information
            existingUser.Username = user.Username;
            existingUser.Email = user.Email;

            try
            {
                _context.Update(existingUser);
                await _context.SaveChangesAsync();

                // Update session information
                HttpContext.Session.SetString("Username", existingUser.Username);

                ViewBag.Success = "Profile updated successfully!";
                return View(existingUser);
            }
            catch (DbUpdateConcurrencyException)
            {
                ViewBag.Error = "An error occurred while updating your profile. Please try again.";
                return View(existingUser);
            }
        }

        // GET: Users/Edit/5 - Admin only
        public async Task<IActionResult> Edit(int? id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");
            
            if (userId == null)
            {
                return RedirectToAction("Index");
            }

            // Only admin can edit other users, regular users should use EditProfile
            if (userRole != "Admin")
            {
                return RedirectToAction("EditProfile");
            }

            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        // POST: Users/Edit/5 - Admin only
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Username,Email,PasswordHash,Role")] Users user, bool changePassword = false)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");
            
            if (userId == null)
            {
                return RedirectToAction("Index");
            }

            // Only admin can edit other users
            if (userRole != "Admin")
            {
                return RedirectToAction("EditProfile");
            }

            if (id != user.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingUser = await _context.Users.FindAsync(id);
                    if (existingUser == null)
                    {
                        return NotFound();
                    }

                    existingUser.Username = user.Username;
                    existingUser.Email = user.Email;
                    existingUser.Role = user.Role;

                    // Only update password if changePassword is true and new password is provided
                    if (changePassword && !string.IsNullOrEmpty(user.PasswordHash))
                    {
                        existingUser.PasswordHash = HashPassword(user.PasswordHash);
                    }

                    _context.Update(existingUser);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(user.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Manage));
            }
            return View(user);
        }

        // GET: Users/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Index");
            }

            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Index");
            }

            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Manage));
        }

        // Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }

        // GET: Users/Profile - User's own profile
        public async Task<IActionResult> Profile()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Index");
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return RedirectToAction("Logout");
            }

            return View(user);
        }

        // POST: Users/UpdateProfile - Update user's own profile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile([Bind("Id,Username,Email")] Users user, string currentPassword, string newPassword, string confirmPassword)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null || userId != user.Id)
            {
                return RedirectToAction("Index");
            }

            var existingUser = await _context.Users.FindAsync(userId);
            if (existingUser == null)
            {
                return RedirectToAction("Logout");
            }

            // Validate current password if trying to change password
            if (!string.IsNullOrEmpty(newPassword))
            {
                if (string.IsNullOrEmpty(currentPassword) || HashPassword(currentPassword) != existingUser.PasswordHash)
                {
                    ViewBag.Error = "Current password is incorrect.";
                    return View("Profile", existingUser);
                }

                if (newPassword != confirmPassword)
                {
                    ViewBag.Error = "New passwords do not match.";
                    return View("Profile", existingUser);
                }

                if (newPassword.Length < 6)
                {
                    ViewBag.Error = "Password must be at least 6 characters long.";
                    return View("Profile", existingUser);
                }

                existingUser.PasswordHash = HashPassword(newPassword);
            }

            // Update other profile information
            existingUser.Username = user.Username;
            existingUser.Email = user.Email;

            try
            {
                _context.Update(existingUser);
                await _context.SaveChangesAsync();

                // Update session information
                HttpContext.Session.SetString("Username", existingUser.Username);

                ViewBag.Success = "Profile updated successfully!";
            }
            catch (DbUpdateConcurrencyException)
            {
                ViewBag.Error = "An error occurred while updating your profile.";
            }

            return View("Profile", existingUser);
        }

        // GET: Users/QuizHistory - User's quiz history
        public async Task<IActionResult> QuizHistory()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Index");
            }

            var quizSessions = await _context.QuizSessions
                .Where(qs => qs.UserId == userId)
                .OrderByDescending(qs => qs.StartTime)
                .ToListAsync();

            return View(quizSessions);
        }

        // GET: Users/GetSessionDetails - Get detailed session information via AJAX
        [HttpGet]
        public async Task<IActionResult> GetSessionDetails(int sessionId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return Json(new { success = false, message = "Not authenticated" });
            }

            var session = await _context.QuizSessions
                .Include(s => s.UserAnswers!)
                .ThenInclude(ua => ua.Question!)
                .ThenInclude(q => q.Category)
                .FirstOrDefaultAsync(s => s.SessionId == sessionId && s.UserId == userId);

            if (session == null)
            {
                return Json(new { success = false, message = "Session not found" });
            }

            var totalQuestions = session.NumberOfQuestions;
            var answeredQuestions = session.UserAnswers?.Count ?? 0;
            var correctAnswers = session.UserAnswers?.Count(ua => ua.IsCorrect) ?? 0;
            var wrongAnswers = answeredQuestions - correctAnswers;
            
            var duration = session.EndTime?.Subtract(session.StartTime);
            var durationText = duration?.TotalMinutes > 0 
                ? $"{(int)duration.Value.TotalMinutes}m {duration.Value.Seconds}s" 
                : "In Progress";

            var categories = session.UserAnswers?
                .Select(ua => ua.Question?.Category?.CategoryName)
                .Where(c => !string.IsNullOrEmpty(c))
                .Distinct()
                .ToList() ?? new List<string?>();

            var scorePercentage = totalQuestions > 0 ? Math.Round((double)correctAnswers / totalQuestions * 100, 1) : 0;

            return Json(new
            {
                success = true,
                sessionId = session.SessionId,
                quizTitle = session.QuizTitle,
                startTime = session.StartTime.ToString("MMM dd, yyyy HH:mm:ss"),
                endTime = session.EndTime?.ToString("MMM dd, yyyy HH:mm:ss") ?? "Not finished",
                duration = durationText,
                isCompleted = session.IsCompleted,
                timeLimit = session.TimeLimit,
                totalQuestions = totalQuestions,
                answeredQuestions = answeredQuestions,
                correctAnswers = correctAnswers,
                wrongAnswers = wrongAnswers,
                unansweredQuestions = totalQuestions - answeredQuestions,
                scorePercentage = scorePercentage,
                difficultyLevel = session.DifficultyLevel ?? "Mixed",
                categories = categories
            });
        }

        // GET: Users/ContinueQuiz - Continue an incomplete quiz session
        public async Task<IActionResult> ContinueQuiz(int sessionId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Index");
            }

            var session = await _context.QuizSessions
                .FirstOrDefaultAsync(s => s.SessionId == sessionId && s.UserId == userId && !s.IsCompleted);

            if (session == null)
            {
                TempData["Error"] = "Quiz session not found or already completed.";
                return RedirectToAction("QuizHistory");
            }

            // Check if session has expired
            var timeElapsed = (DateTime.Now - session.StartTime).TotalMinutes;
            if (timeElapsed > session.TimeLimit)
            {
                // Mark as completed due to timeout
                session.IsCompleted = true;
                session.EndTime = DateTime.Now;
                _context.Update(session);
                await _context.SaveChangesAsync();

                TempData["Warning"] = "This quiz session has expired and has been marked as completed.";
                return RedirectToAction("QuizHistory");
            }

            // Find the next unanswered question
            var answeredQuestionIds = await _context.UserAnswers
                .Where(ua => ua.SessionId == sessionId)
                .Select(ua => ua.QuestionId)
                .ToListAsync();

            var nextQuestionIndex = answeredQuestionIds.Count;

            // Redirect to quiz question page
            return RedirectToAction("Question", "Quiz", new { sessionId = sessionId, questionIndex = nextQuestionIndex });
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }

        private string HashPassword(string password)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}
