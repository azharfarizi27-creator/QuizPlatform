using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuizPlatform.API.Models.Entity
{
    [Table("Leaderboards")]
    public class Leaderboard
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }

        public int QuizId { get; set; }

        public int Score { get; set; }

        public int DurationInSeconds { get; set; }

        public int RankPosition { get; set; }

        public DateTime CreatedAt { get; set; }

        
        
    }
}