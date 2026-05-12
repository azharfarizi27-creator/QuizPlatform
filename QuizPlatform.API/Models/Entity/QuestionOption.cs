using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuizPlatform.API.Models.Entity
{
    [Table("QuestionOptions")]
    public class QuestionOption
    {
        [Key]
        public int Id { get; set; }

        public int QuestionId { get; set; }

        public string OptionText { get; set; }

        public bool IsCorrect { get; set; }

        public int OrderNumber { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}