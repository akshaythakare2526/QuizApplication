using System.ComponentModel.DataAnnotations;

namespace QuizApplication.Models
{
    public class Categories
    {
        [Key]
        public int CategoryId { get; set; }
        [Required]
        public string CategoryName { get; set; }
    }
}