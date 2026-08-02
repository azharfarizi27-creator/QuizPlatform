using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuizPlatform.API.Models.Dtos
{
    public class CreateSuspiciousActivityDto
    {

        public int AttemptId { get; set; }

        public string Reason { get; set; }

        public int WarningCount { get; set; }
    }
}