using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuizApplication.Models
{
    public class Questions
    {
        [Key]
        public int QuestionId { get; set; }
        
        [Required]
        public int CategoryId { get; set; }
        
        [Required]
        [Display(Name = "Question Text")]
        public required string QuestionText { get; set; }
        
        // Store image as binary data in database
        [Display(Name = "Question Image")]
        public byte[]? QuestionImageData { get; set; }
        
        [Display(Name = "Image Content Type")]
        public string? ImageContentType { get; set; }
        
        [Required]
        [Display(Name = "Option A")]
        public required string Option1 { get; set; }
        
        [Required]
        [Display(Name = "Option B")]
        public required string Option2 { get; set; }
        
        [Required]
        [Display(Name = "Option C")]
        public required string Option3 { get; set; }
        
        [Required]
        [Display(Name = "Option D")]
        public required string Option4 { get; set; }
        
        [Required]
        [Display(Name = "Correct Answer")]
        public required string CorrectedOption { get; set; }
        
        [Required]
        [Display(Name = "Difficulty Level")]
        public required string DifficultyLevel { get; set; }
        
        // Navigation property
        [ForeignKey("CategoryId")]
        public virtual Categories? Category { get; set; }
        
        // Not mapped property for file upload
        [NotMapped]
        [Display(Name = "Upload Image")]
        public IFormFile? ImageFile { get; set; }
    }
}