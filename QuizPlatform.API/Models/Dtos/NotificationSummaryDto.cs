using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuizPlatform.API.Models.Dtos
{
    public class NotificationSummaryDto
    {

        public int PendingFriendRequests { get; set; }

        public int UnreadMessages { get; set; }

        public int PendingQuizzes { get; set; }

        public int DueSoonQuizzes { get; set; }

        public int ExpiredQuizzes { get; set; }

        public int Total { get; set; }
    }
}