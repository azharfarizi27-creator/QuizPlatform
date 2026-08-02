using System;

namespace QuizPlatform.API.Models.Entity
{
    public class QuizSuspiciousActivity
    {
        public int Id { get; set; }

        public int AttemptId { get; set; }

        public int UserId { get; set; }

        public int QuizId { get; set; }

        public string Reason { get; set; }

        public int WarningCount { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}