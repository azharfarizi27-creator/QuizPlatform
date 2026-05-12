using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuizPlatform.API.Models.Entity
{
    [Table("UserAnswers")]
    public class UserAnswer
    {
        [Key]
        public int Id { get; set; }

        public int AttemptId { get; set; }

        public int QuestionId { get; set; }

        public int? QuestionOptionId { get; set; }

        public string EssayAnswer { get; set; }

        public bool? IsCorrect { get; set; }

        public int? EarnedScore { get; set; }

        public DateTime AnsweredAt { get; set; }
    }
}