using System.ComponentModel.DataAnnotations;

namespace QuizApplication.ViewModels
{
    public class PracticeSetupViewModel
    {
        [Required]
        public string PracticeMode { get; set; } = ""; // "difficulty" or "category"
        
        public string? DifficultyLevel { get; set; }
        
        public List<int> SelectedCategoryIds { get; set; } = new List<int>();
        
        public List<CategoryOption> AvailableCategories { get; set; } = new List<CategoryOption>();
    }

    public class PracticeQuestionViewModel
    {
        public int QuestionId { get; set; }
        public string QuestionText { get; set; } = "";
        public byte[]? QuestionImageData { get; set; }
        public string Option1 { get; set; } = "";
        public string Option2 { get; set; } = "";
        public string Option3 { get; set; } = "";
        public string Option4 { get; set; } = "";
        public string CorrectAnswer { get; set; } = "";
        public string DifficultyLevel { get; set; } = "";
        public string CategoryName { get; set; } = "";
        public string? SelectedOption { get; set; }
        public bool ShowAnswer { get; set; } = false;
        public string? Explanation { get; set; }
    }
}
