using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuizApplication.Models
{
    public class QuizSessions
    {
        [Key]
        public int SessionId { get; set; }
        
        [Required]
        public int UserId { get; set; }
        
        [Required]
        public DateTime StartTime { get; set; }
        
        public DateTime? EndTime { get; set; }
        
        [Required]
        public bool IsCompleted { get; set; }
        
        [Required]
        [Display(Name = "Time Limit (Minutes)")]
        public int TimeLimit { get; set; }
        
        [Display(Name = "Number of Questions")]
        public int NumberOfQuestions { get; set; }
        
        [Display(Name = "Difficulty Level")]
        public string? DifficultyLevel { get; set; }
        
        [Display(Name = "Selected Categories")]
        public string? SelectedCategories { get; set; } // JSON string of category IDs
        
        [Display(Name = "Total Score")]
        public int TotalScore { get; set; }
        
        [Display(Name = "Max Possible Score")]
        public int MaxPossibleScore { get; set; }
        
        [Display(Name = "Quiz Title")]
        public string? QuizTitle { get; set; }
        
        // Navigation properties
        [ForeignKey("UserId")]
        public virtual Users? User { get; set; }
        
        public virtual ICollection<UserAnswers> UserAnswers { get; set; } = new List<UserAnswers>();
    }
}