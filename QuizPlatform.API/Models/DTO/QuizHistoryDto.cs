using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuizPlatform.API.Models.DTO
{
    public class QuizHistoryDto
    {
        public int AttemptId { get; set; }
        public int QuizId { get; set; }
        public string QuizTitle { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int TotalScore { get; set; }
        public bool Status { get; set; }
    }
}