using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuizPlatform.API.Models.Dtos.Support
{
    public class SupportMessageDto
    {
        public int Id { get; set; }

        public int TicketId { get; set; }

        public int SenderUserId { get; set; }

        public string SenderName { get; set; }

        public string Message { get; set; }

        public bool IsAdmin { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}