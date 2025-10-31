using System.Collections.Generic;

namespace QuizApplication.ViewModels
{
    public class WelcomeViewModel
    {
        public List<TopScoreViewModel> TopScores { get; set; } = new List<TopScoreViewModel>();
    }

    public class TopScoreViewModel
    {
        public string Username { get; set; } = string.Empty;
        public int Score { get; set; }
        public int MaxScore { get; set; }
        public DateTime CompletedDate { get; set; }
        public double Percentage { get; set; }
    }
}
