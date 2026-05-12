using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuizPlatform.API.Models.DTO
{
    public class QuizResultDetailDto
    {
        public int QuestionId { get; set; }
        public string QuestionText { get; set; }

        public string UserAnswer { get; set; }
        public string CorrectAnswer { get; set; }

        public bool IsCorrect { get; set; }
        public int Score { get; set; }
    }
}