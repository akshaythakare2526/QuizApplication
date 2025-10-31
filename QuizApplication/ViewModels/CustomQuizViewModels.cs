using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QuizApplication.Models;

namespace QuizApplication.ViewModels
{
    // ViewModel for creating a new custom quiz
    public class CreateCustomQuizViewModel
    {
        [Required]
        [StringLength(200)]
        [Display(Name = "Quiz Title")]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Required]
        [Display(Name = "Time Limit (Minutes)")]
        [Range(1, 180, ErrorMessage = "Time limit must be between 1 and 180 minutes")]
        public int TimeLimit { get; set; } = 30;

        [Display(Name = "Make Public (Anyone can take this quiz)")]
        public bool IsPublic { get; set; } = true;

        [Display(Name = "Category")]
        public int? CategoryId { get; set; }

        [Display(Name = "Difficulty Level")]
        public string? DifficultyLevel { get; set; }

        // List of available categories
        public List<Categories>? AvailableCategories { get; set; }
    }

    // ViewModel for selecting questions for a custom quiz
    public class SelectQuestionsViewModel
    {
        public int CustomQuizId { get; set; }
        public string QuizTitle { get; set; } = string.Empty;
        public List<QuestionSelectionItem> AvailableQuestions { get; set; } = new List<QuestionSelectionItem>();
        public List<int> SelectedQuestionIds { get; set; } = new List<int>();
    }

    public class QuestionSelectionItem
    {
        public int QuestionId { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public string? CategoryName { get; set; }
        public string? DifficultyLevel { get; set; }
        public bool IsSelected { get; set; }
    }

    // ViewModel for assigning quiz to users
    public class AssignQuizViewModel
    {
        public int CustomQuizId { get; set; }
        public string QuizTitle { get; set; } = string.Empty;
        public List<UserSelectionItem> AvailableUsers { get; set; } = new List<UserSelectionItem>();
        public List<int> SelectedUserIds { get; set; } = new List<int>();
    }

    public class UserSelectionItem
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool IsSelected { get; set; }
    }

    // ViewModel for displaying custom quizzes
    public class CustomQuizListViewModel
    {
        public int CustomQuizId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public int CreatedByUserId { get; set; }
        public DateTime CreatedDate { get; set; }
        public int TimeLimit { get; set; }
        public bool IsPublic { get; set; }
        public int QuestionCount { get; set; }
        public string? CategoryName { get; set; }
        public string? DifficultyLevel { get; set; }
        public bool IsAssigned { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsViewed { get; set; }
        public int? Score { get; set; }
    }

    // ViewModel for quiz details
    public class CustomQuizDetailsViewModel
    {
        public int CustomQuizId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public int TimeLimit { get; set; }
        public bool IsPublic { get; set; }
        public string? CategoryName { get; set; }
        public string? DifficultyLevel { get; set; }
        public int QuestionCount { get; set; }
        public List<AssignedUserInfo> AssignedUsers { get; set; } = new List<AssignedUserInfo>();
        public bool CanEdit { get; set; }
        public bool CanTake { get; set; }
    }

    public class AssignedUserInfo
    {
        public string Username { get; set; } = string.Empty;
        public DateTime AssignedDate { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime? CompletedDate { get; set; }
        public int? Score { get; set; }
    }

    // ViewModel for taking a custom quiz
    public class TakeCustomQuizViewModel
    {
        public int CustomQuizId { get; set; }
        public int AssignmentId { get; set; }
        public string Title { get; set; } = string.Empty;
        public int TimeLimit { get; set; }
        public DateTime StartTime { get; set; }
        public List<CustomQuizQuestionItem> Questions { get; set; } = new List<CustomQuizQuestionItem>();
    }

    public class CustomQuizQuestionItem
    {
        public int QuestionId { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public string? ImageData { get; set; }
        public string Option1 { get; set; } = string.Empty;
        public string Option2 { get; set; } = string.Empty;
        public string Option3 { get; set; } = string.Empty;
        public string Option4 { get; set; } = string.Empty;
        public int QuestionOrder { get; set; }
    }

    // ViewModel for dashboard notifications
    public class CustomQuizNotificationViewModel
    {
        public int AssignmentId { get; set; }
        public int CustomQuizId { get; set; }
        public string QuizTitle { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime AssignedDate { get; set; }
        public int TimeLimit { get; set; }
        public int QuestionCount { get; set; }
        public bool IsViewed { get; set; }
    }
}
