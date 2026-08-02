using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuizPlatform.API.Models.DTO
{
    public class QuestionAnalyticsDto
    {

        public int QuestionId { get; set; }

        public string QuestionText { get; set; }

        public string QuizTitle { get; set; }

        public int TotalAnswered { get; set; }

        public int TotalCorrect { get; set; }

        public int TotalWrong { get; set; }

        public double CorrectPercentage { get; set; }
    }
}