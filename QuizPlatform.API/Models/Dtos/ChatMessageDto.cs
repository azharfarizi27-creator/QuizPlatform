using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuizPlatform.API.Models.Dtos
{
    public class ChatMessageDto
    {

        public int Id { get; set; }

        public int SenderId { get; set; }

        public string SenderName { get; set; }

        public string SenderImage { get; set; }

        public int ReceiverId { get; set; }

        public string ReceiverName { get; set; }

        public string ReceiverImage { get; set; }

        public string MessageText { get; set; }

        public bool IsMine { get; set; }

        public bool IsRead { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}