using System;

namespace QuizPlatform.API.Models.DTO
{
    public class AntiCheatLogDto
    {
        public int Id { get; set; }

        public int AttemptId { get; set; }

        public int UserId { get; set; }

        public string FullName { get; set; }

        public string Username { get; set; }

        public int QuizId { get; set; }

        public string QuizTitle { get; set; }

        public string Reason { get; set; }

        public int WarningCount { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}