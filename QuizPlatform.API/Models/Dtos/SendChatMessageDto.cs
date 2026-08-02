using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuizPlatform.API.Models.Dtos
{
    public class SendChatMessageDto
    {

        public int ReceiverId { get; set; }

        public string MessageText { get; set; }
    }
}