using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuizPlatform.API.Models.Dtos
{
    public class FriendRequestDto
    {

        public int Id { get; set; }

        public int RequesterId { get; set; }

        public string RequesterName { get; set; }

        public string RequesterUsername { get; set; }

        public string RequesterRole { get; set; }

        public string RequesterImage { get; set; }

        public int ReceiverId { get; set; }

        public string ReceiverName { get; set; }

        public string ReceiverUsername { get; set; }

        public string ReceiverRole { get; set; }

        public string ReceiverImage { get; set; }

        public string Status { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? RespondedAt { get; set; }
    }
}