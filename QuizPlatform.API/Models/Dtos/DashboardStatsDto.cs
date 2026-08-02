using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuizPlatform.API.Models.DTO
{
    public class DashboardStatsDto
    {

        public int TotalUsers { get; set; }

        public int TotalStudents { get; set; }

        public int TotalTeachers { get; set; }

        public int TotalQuizzes { get; set; }

        public int TotalQuestions { get; set; }

        public int TotalAttempts { get; set; }
    }
}