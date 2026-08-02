using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuizPlatform.API.Models.Dtos
{
    public class FriendDto
    {
        public int UserId { get; set; }

        public string FullName { get; set; }

        public string Username { get; set; }

        public string Email { get; set; }

        public string RoleName { get; set; }

        public string ProfileImage { get; set; }

        public int UnreadCount { get; set; }

        public string LastMessageText { get; set; }

        public DateTime? LastMessageAt { get; set; }
    }
}