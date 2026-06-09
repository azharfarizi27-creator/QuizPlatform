using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuizPlatform.API.Models.DTO
{
    public class StudentNotificationDto
    {

        public int QuizId { get; set; }

        public string QuizTitle { get; set; }

        public string Message { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}