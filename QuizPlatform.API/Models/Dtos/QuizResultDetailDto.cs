using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuizPlatform.API.Models.DTO
{
    public class QuizResultDetailDto
    {
  public int QuestionId { get; set; }

        public int QuestionTypeId { get; set; }

        public string QuestionType { get; set; }

        public string QuestionText { get; set; }

        public string UserAnswer { get; set; }

        public string SelectedAnswer { get; set; }

        public string CorrectAnswer { get; set; }

        public string Explanation { get; set; }

        public bool? IsCorrect { get; set; }

        public int Score { get; set; }

        public int MaxScore { get; set; }
    }
}