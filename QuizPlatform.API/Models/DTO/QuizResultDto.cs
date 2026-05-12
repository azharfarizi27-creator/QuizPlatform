using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuizPlatform.API.Models.DTO
{
    public class QuizResultDto
    {
        public int AttemptId { get; set; }
        public int UserId { get; set; }
        public int QuizId { get; set; }

        public int TotalScore { get; set; }
        public int TotalCorrect { get; set; }
        public int TotalWrong { get; set; }

        public int DurationInSeconds { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public List<QuizResultDetailDto> Details { get; set; }
    }
}