using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace QuizPlatform.API.Models.Entity
{
    [Table("SupportMessages")]
    public class SupportMessage
    {
        [Key]
        public int Id { get; set; }

        public int TicketId { get; set; }

        public int SenderUserId { get; set; }

        [Required]
        public string Message { get; set; }

        public bool IsAdmin { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}