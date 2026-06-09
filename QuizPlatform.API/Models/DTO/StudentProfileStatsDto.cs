using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Web;

namespace QuizPlatform.API.Models.DTO
{
    
        public class StudentProfileStatsDto
        {
            public int Id { get; set; }

            public string FullName { get; set; }

            public string Username { get; set; }

            public string Email { get; set; }

            public string ProfileImage { get; set; }

            public int TotalQuiz { get; set; }

            public int PassedQuiz { get; set; }

            public int FailedQuiz { get; set; }

            public int HighestScore { get; set; }

            public double AverageScore { get; set; }
        }
    
}