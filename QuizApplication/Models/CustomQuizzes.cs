using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuizApplication.Models
{
    public class CustomQuizzes
    {
        [Key]
        public int CustomQuizId { get; set; }

        [Required]
        [StringLength(200)]
        [Display(Name = "Quiz Title")]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Required]
        [Display(Name = "Created By")]
        public int CreatedByUserId { get; set; }

        [Required]
        [Display(Name = "Time Limit (Minutes)")]
        [Range(1, 180)]
        public int TimeLimit { get; set; }

        [Required]
        [Display(Name = "Is Public")]
        public bool IsPublic { get; set; } = true;

        [Required]
        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [Display(Name = "Category")]
        public int? CategoryId { get; set; }

        [Display(Name = "Difficulty Level")]
        [StringLength(20)]
        public string? DifficultyLevel { get; set; }

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;

        // Navigation properties
        [ForeignKey("CreatedByUserId")]
        public virtual Users? CreatedBy { get; set; }

        [ForeignKey("CategoryId")]
        public virtual Categories? Category { get; set; }

        public virtual ICollection<CustomQuizQuestions> CustomQuizQuestions { get; set; } = new List<CustomQuizQuestions>();
        public virtual ICollection<CustomQuizAssignments> CustomQuizAssignments { get; set; } = new List<CustomQuizAssignments>();
    }

    // Join table for Quiz and Questions
    public class CustomQuizQuestions
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CustomQuizId { get; set; }

        [Required]
        public int QuestionId { get; set; }

        [Display(Name = "Order")]
        public int QuestionOrder { get; set; }

        // Navigation properties
        [ForeignKey("CustomQuizId")]
        public virtual CustomQuizzes? CustomQuiz { get; set; }

        [ForeignKey("QuestionId")]
        public virtual Questions? Question { get; set; }
    }

    // Track which users are assigned to which custom quizzes
    public class CustomQuizAssignments
    {
        [Key]
        public int AssignmentId { get; set; }

        [Required]
        public int CustomQuizId { get; set; }

        [Required]
        public int AssignedToUserId { get; set; }

        [Required]
        public DateTime AssignedDate { get; set; } = DateTime.Now;

        [Display(Name = "Is Completed")]
        public bool IsCompleted { get; set; } = false;

        [Display(Name = "Completed Date")]
        public DateTime? CompletedDate { get; set; }

        [Display(Name = "Is Viewed")]
        public bool IsViewed { get; set; } = false;

        [Display(Name = "Score")]
        public int? Score { get; set; }

        // Navigation properties
        [ForeignKey("CustomQuizId")]
        public virtual CustomQuizzes? CustomQuiz { get; set; }

        [ForeignKey("AssignedToUserId")]
        public virtual Users? AssignedToUser { get; set; }
    }
}
