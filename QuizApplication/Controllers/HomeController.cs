using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuizApplication.Data;
using QuizApplication.ViewModels;
using System.Linq;

namespace QuizApplication.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Home/Index - Welcome Page
        public async Task<IActionResult> Index()
        {
            var viewModel = new WelcomeViewModel();

            // Get top 5 scores from completed quiz sessions
            var topScores = await _context.QuizSessions
                .Include(q => q.User)
                .Where(q => q.IsCompleted && q.EndTime != null)
                .OrderByDescending(q => q.TotalScore)
                .ThenByDescending(q => q.EndTime)
                .Take(5)
                .Select(q => new TopScoreViewModel
                {
                    Username = q.User != null ? q.User.Username : "Anonymous",
                    Score = q.TotalScore,
                    MaxScore = q.MaxPossibleScore,
                    CompletedDate = q.EndTime ?? DateTime.Now,
                    Percentage = q.MaxPossibleScore > 0 
                        ? Math.Round((double)q.TotalScore / q.MaxPossibleScore * 100, 2) 
                        : 0
                })
                .ToListAsync();

            viewModel.TopScores = topScores;

            return View(viewModel);
        }
    }
}
