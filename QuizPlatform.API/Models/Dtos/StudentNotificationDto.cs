using System;

namespace QuizPlatform.API.Models.DTO
{
    public class StudentNotificationDto
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public int QuizId { get; set; }

        public string QuizTitle { get; set; }

        public string Title { get; set; }

        public string Message { get; set; }

        public string Link { get; set; }

        public string NotificationType { get; set; }

        public int? ReferenceId { get; set; }

        public bool IsRead { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}