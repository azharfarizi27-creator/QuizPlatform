using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuizPlatform.API.Models.Entity
{
    [Table("Quizzes")]
    public class Quiz
    {
        [Key]
        public int Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public int CategoryId { get; set; }

        public int LevelId { get; set; }

        public int DifficultyId { get; set; }

        public int CreatedBy { get; set; }

        public int DurationInMinutes { get; set; }

        public int PassingScore { get; set; }

        public string Thumbnail { get; set; }

        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}