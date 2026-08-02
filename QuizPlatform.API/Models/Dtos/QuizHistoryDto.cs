using System;

namespace QuizPlatform.API.Models.DTO
{
    public class QuizHistoryDto
    {
        public int AttemptId { get; set; }

        public int UserId { get; set; }

        public int QuizId { get; set; }

        public string QuizTitle { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        public int TotalScore { get; set; }

        public bool Status { get; set; }

        public int PassingScore { get; set; }

        public int DurationInSeconds { get; set; }

        public bool? IsPassed { get; set; }

        public bool HasPendingEssay { get; set; }

        public int PendingEssayCount { get; set; }

        public string ResultStatus { get; set; }
    }
}