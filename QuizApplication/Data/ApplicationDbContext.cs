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
        
        // User Custom Quiz Tables
        public DbSet<UserCustomQuiz> UserCustomQuizzes { get; set; }
        public DbSet<UserCustomQuizQuestion> UserCustomQuizQuestions { get; set; }
        public DbSet<UserCustomQuizAssignment> UserCustomQuizAssignments { get; set; }
        public DbSet<UserCustomQuizAnswer> UserCustomQuizAnswers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure cascade delete behavior to avoid multiple cascade paths
            modelBuilder.Entity<UserCustomQuizAssignment>()
                .HasOne(a => a.AssignedToUser)
                .WithMany()
                .HasForeignKey(a => a.AssignedToUserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<UserCustomQuizAssignment>()
                .HasOne(a => a.UserQuiz)
                .WithMany(q => q.Assignments)
                .HasForeignKey(a => a.UserQuizId)
                .OnDelete(DeleteBehavior.Cascade);
                
            modelBuilder.Entity<UserCustomQuizAnswer>()
                .HasOne(a => a.Assignment)
                .WithMany()
                .HasForeignKey(a => a.AssignmentId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<UserCustomQuizAnswer>()
                .HasOne(a => a.Question)
                .WithMany()
                .HasForeignKey(a => a.QuestionId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}