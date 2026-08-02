using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace QuizPlatform.API.Models.Entity
{
    [Table("EmailOtps")]
    public class EmailOtp
    {
        [Key]
        public int Id { get; set; }

        public int? UserId { get; set; }

        public string Email { get; set; }

        public string Code { get; set; }

        public string Purpose { get; set; }

        public bool IsUsed { get; set; }

        public DateTime ExpiredAt { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}