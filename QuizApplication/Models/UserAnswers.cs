using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuizApplication.Models
{
    public class UserAnswers
    {
        [Key]
        public int AnswerId { get; set; }
        
        [Required]
        public int SessionId { get; set; }
        
        [Required]
        public int QuestionId { get; set; }
        
        [Required]
        [Display(Name = "Selected Option")]
        public required string SelectedOption { get; set; }
        
        [Required]
        [Display(Name = "Is Correct")]
        public bool IsCorrect { get; set; }
        
        [Display(Name = "Answer Time")]
        public DateTime? AnsweredAt { get; set; }
        
        [Display(Name = "Time Taken (Seconds)")]
        public int? TimeTaken { get; set; }
        
        // Navigation properties
        [ForeignKey("SessionId")]
        public virtual QuizSessions? QuizSession { get; set; }
        
        [ForeignKey("QuestionId")]
        public virtual Questions? Question { get; set; }
    }
}