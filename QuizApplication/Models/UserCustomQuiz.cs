using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuizApplication.Models
{
    // Main table for user-created custom quizzes
    public class UserCustomQuiz
    {
        [Key]
        public int UserQuizId { get; set; }

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

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;

        // Navigation properties
        [ForeignKey("CreatedByUserId")]
        public virtual Users? CreatedBy { get; set; }

        public virtual ICollection<UserCustomQuizQuestion> Questions { get; set; } = new List<UserCustomQuizQuestion>();
        public virtual ICollection<UserCustomQuizAssignment> Assignments { get; set; } = new List<UserCustomQuizAssignment>();
    }

    // Table for questions within user custom quizzes
    public class UserCustomQuizQuestion
    {
        [Key]
        public int QuestionId { get; set; }

        [Required]
        public int UserQuizId { get; set; }

        [Required]
        [Display(Name = "Question Text")]
        [StringLength(1000)]
        public string QuestionText { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Option A")]
        [StringLength(500)]
        public string Option1 { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Option B")]
        [StringLength(500)]
        public string Option2 { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Option C")]
        [StringLength(500)]
        public string Option3 { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Option D")]
        [StringLength(500)]
        public string Option4 { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Correct Answer")]
        [StringLength(10)]
        public string CorrectAnswer { get; set; } = string.Empty; // "Option1", "Option2", "Option3", or "Option4"

        [Display(Name = "Question Order")]
        public int QuestionOrder { get; set; }

        // Navigation property
        [ForeignKey("UserQuizId")]
        public virtual UserCustomQuiz? UserQuiz { get; set; }
    }

    // Table for tracking quiz assignments to specific users
    public class UserCustomQuizAssignment
    {
        [Key]
        public int AssignmentId { get; set; }

        [Required]
        public int UserQuizId { get; set; }

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

        [Display(Name = "Total Questions")]
        public int? TotalQuestions { get; set; }

        // Navigation properties
        [ForeignKey("UserQuizId")]
        public virtual UserCustomQuiz? UserQuiz { get; set; }

        [ForeignKey("AssignedToUserId")]
        public virtual Users? AssignedToUser { get; set; }
    }

    // Table for storing user answers to custom quiz questions
    public class UserCustomQuizAnswer
    {
        [Key]
        public int AnswerId { get; set; }

        [Required]
        public int AssignmentId { get; set; }

        [Required]
        public int QuestionId { get; set; }

        [Required]
        [StringLength(10)]
        public string SelectedAnswer { get; set; } = string.Empty; // "Option1", "Option2", "Option3", or "Option4"

        [Display(Name = "Is Correct")]
        public bool IsCorrect { get; set; }

        [Display(Name = "Answered Date")]
        public DateTime AnsweredDate { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("AssignmentId")]
        public virtual UserCustomQuizAssignment? Assignment { get; set; }

        [ForeignKey("QuestionId")]
        public virtual UserCustomQuizQuestion? Question { get; set; }
    }
}
