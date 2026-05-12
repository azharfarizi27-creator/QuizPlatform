using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuizPlatform.API.Models.Entity
{
    [Table("QuizAttempts")]
    public class QuizAttempt
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }

        public int QuizId { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        public int TotalScore { get; set; }

        public bool Status { get; set; }
    }
}