using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuizPlatform.API.Models.Entity
{
    [Table("Questions")]
    public class Question
    {
        [Key]
        public int Id { get; set; }

        public int QuizId { get; set; }

        public int QuestionTypeId { get; set; }

        public string QuestionText { get; set; }

        public string QuestionImage { get; set; }

        public string Explanation { get; set; }

        public int Score { get; set; }

        public int OrderNumber { get; set; }

        public DateTime CreatedAt { get; set; }

        public int? QuestionBankId { get; set; }

    }
}