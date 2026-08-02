using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuizPlatform.API.Models.DTO
{
    public class EssayPendingDto
    {
        public int AnswerId { get; set; }

        public int AttemptId { get; set; }

        public int UserId { get; set; }

        public string StudentName { get; set; }

        public int QuizId { get; set; }

        public string QuizTitle { get; set; }

        public int QuestionId { get; set; }

        public string QuestionText { get; set; }

        public string EssayAnswer { get; set; }

        public int MaxScore { get; set; }

        public int? EarnedScore { get; set; }

        public bool? IsCorrect { get; set; }
    }

    public class GradeEssayDto
    {
        public int AnswerId { get; set; }

        public int EarnedScore { get; set; }

        public bool IsCorrect { get; set; }
    }
}