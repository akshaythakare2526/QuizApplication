using System.ComponentModel.DataAnnotations;

namespace QuizApplication.ViewModels
{
    public class QuizSetupViewModel
    {
        [Required]
        [Display(Name = "Quiz Title")]
        public string QuizTitle { get; set; } = "";

        [Required]
        [Display(Name = "Number of Questions")]
        [Range(1, 100, ErrorMessage = "Please select between 1 and 100 questions")]
        public int NumberOfQuestions { get; set; } = 10;

        [Required]
        [Display(Name = "Time Limit (Minutes)")]
        [Range(1, 180, ErrorMessage = "Please select between 1 and 180 minutes")]
        public int TimeLimit { get; set; } = 30;

        [Display(Name = "Difficulty Level")]
        public string? DifficultyLevel { get; set; }

        [Display(Name = "Select Categories")]
        public List<int> SelectedCategoryIds { get; set; } = new List<int>();

        // For displaying available categories
        public List<CategoryOption> AvailableCategories { get; set; } = new List<CategoryOption>();
    }

    public class CategoryOption
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = "";
        public int QuestionCount { get; set; }
        public bool IsSelected { get; set; }
    }

    public class QuizQuestionViewModel
    {
        public int SessionId { get; set; }
        public int QuestionId { get; set; }
        public int CurrentQuestionIndex { get; set; }
        public int TotalQuestions { get; set; }
        public string QuestionText { get; set; } = "";
        public byte[]? QuestionImageData { get; set; }
        public string? ImageContentType { get; set; }
        public string Option1 { get; set; } = "";
        public string Option2 { get; set; } = "";
        public string Option3 { get; set; } = "";
        public string Option4 { get; set; } = "";
        public string? SelectedOption { get; set; }
        public string DifficultyLevel { get; set; } = "";
        public string CategoryName { get; set; } = "";
        public int RemainingTimeInSeconds { get; set; }
        public bool HasPrevious { get; set; }
        public bool HasNext { get; set; }
        public List<int> AnsweredQuestions { get; set; } = new List<int>();
        public List<int> AllQuestionIds { get; set; } = new List<int>();
    }

    public class QuizResultViewModel
    {
        public int SessionId { get; set; }
        public string QuizTitle { get; set; } = "";
        public int TotalQuestions { get; set; }
        public int CorrectAnswers { get; set; }
        public int WrongAnswers { get; set; }
        public int UnansweredQuestions { get; set; }
        public double Percentage { get; set; }
        public TimeSpan TimeTaken { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public List<QuestionResultViewModel> QuestionResults { get; set; } = new List<QuestionResultViewModel>();
    }

    public class QuestionResultViewModel
    {
        public int QuestionId { get; set; }
        public string QuestionText { get; set; } = "";
        public string CategoryName { get; set; } = "";
        public string DifficultyLevel { get; set; } = "";
        public string CorrectAnswer { get; set; } = "";
        public string? UserAnswer { get; set; }
        public bool IsCorrect { get; set; }
        public int? TimeTaken { get; set; }
        public string Option1 { get; set; } = "";
        public string Option2 { get; set; } = "";
        public string Option3 { get; set; } = "";
        public string Option4 { get; set; } = "";
    }
}
