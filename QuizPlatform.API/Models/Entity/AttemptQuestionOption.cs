using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace QuizPlatform.API.Models.Entity
{
    [Table("AttemptQuestionOptions")]
    public class AttemptQuestionOption
    {
        [Key]
        public int Id { get; set; }

        public int AttemptId { get; set; }

        public int QuestionId { get; set; }

        public int QuestionOptionId { get; set; }

        public int OrderNumber { get; set; }
    }
}