using System;

namespace QuizPlatform.API.Models.DTO
{
    public class TeacherQuizResultDto
    {
        public int AttemptId { get; set; }

        public string StudentName { get; set; }

        public string QuizTitle { get; set; }

        public int Score { get; set; }

        public int TotalCorrect { get; set; }

        public int TotalWrong { get; set; }

        public int DurationInSeconds { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }
    }
}