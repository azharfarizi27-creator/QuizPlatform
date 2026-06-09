using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuizPlatform.API.Models.DTO
{
    public class TeacherStatsSummaryDto
    {

        public double AverageQuizScore { get; set; }

        public int PassedCount { get; set; }

        public int FailedCount { get; set; }

        public double PassRate { get; set; }
    }
}