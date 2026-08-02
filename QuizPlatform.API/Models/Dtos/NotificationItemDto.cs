using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuizPlatform.API.Models.Dtos
{
    public class NotificationItemDto
    {
        public string Type { get; set; }

        public string Title { get; set; }

        public string Message { get; set; }

        public string TargetUrl { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}