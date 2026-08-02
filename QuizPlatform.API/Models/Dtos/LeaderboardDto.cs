using System;

namespace QuizPlatform.API.Models.DTO
{
    public class LeaderboardDto
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public int QuizId { get; set; }

        public string StudentName { get; set; }

        public int Score { get; set; }

        public int DurationInSeconds { get; set; }

        public int RankPosition { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}