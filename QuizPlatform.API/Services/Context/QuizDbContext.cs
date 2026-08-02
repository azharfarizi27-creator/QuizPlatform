using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Web;
using System.Data.Entity;
using QuizPlatform.API.Models.Entity;

namespace QuizPlatform.API.Services.Context
{
    public class QuizDbContext : DbContext
    {
        public QuizDbContext()
           : base("name=QuizDbContext")
        {
            ((IObjectContextAdapter)this)
                .ObjectContext.CommandTimeout = 600;
        }

        public DbSet<User> Users { get; set; }

        public DbSet<Category> Categories { get; set; }

        public DbSet<Level> Levels { get; set; }

        public DbSet<Difficulty> Difficulties { get; set; }

        public DbSet<Quiz> Quizzes { get; set; }

        public DbSet<Question> Questions { get; set; }

        public DbSet<QuestionOption> QuestionOptions { get; set; }

        public DbSet<UserAnswer> UserAnswers { get; set; }

        public DbSet<QuizAttempt> QuizAttempts { get; set; }

        public DbSet<Leaderboard> Leaderboards { get; set; }

        public DbSet<Role> Roles { get; set; }

        public DbSet<AttemptQuestion> AttemptQuestions { get; set; }

        public DbSet<AttemptQuestionOption> AttemptQuestionOptions { get; set; }

        public DbSet<ActivityLog> ActivityLogs { get; set; }

        public DbSet<QuestionBank> QuestionBanks { get; set; }

        public DbSet<EmailOtp> EmailOtps { get; set; }

        public DbSet<FriendRequest> FriendRequests { get; set; }

        public DbSet<ChatMessage> ChatMessages { get; set; }

        public DbSet<QuizSuspiciousActivity> QuizSuspiciousActivities { get; set; }
        public DbSet<StudentNotification> StudentNotifications { get; set; }

        public DbSet<SupportTicket> SupportTickets { get; set; }
        public DbSet<SupportMessage> SupportMessages { get; set; }
    }
}