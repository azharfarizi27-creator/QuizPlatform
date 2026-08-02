using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuizPlatform.API.Models.Dtos
{
    public class ActivityLogDto
    {

        public int Id { get; set; }

        public int? UserId { get; set; }

        public string FullName { get; set; }

        public string Username { get; set; }

        public string Action { get; set; }

        public string Description { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}