using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuizPlatform.API.Models.DTO
{
    public class TopStudentDto
    {

        public int UserId { get; set; }
        public string StudentName { get; set; }
        public int TotalAttempts { get; set; }
        public int HighestScore { get; set; }
        public double AverageScore { get; set; }

    }
}