using Microsoft.EntityFrameworkCore;
using QuizApplication.Models;

namespace QuizApplication.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Users> Users { get; set; }
        public DbSet<Categories> Categories { get; set; }
        public DbSet<Questions> Questions { get; set; }
        public DbSet<QuizSessions> QuizSessions { get; set; }
        public DbSet<UserAnswers> UserAnswers { get; set; }
    }
}