using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuizPlatform.API.Models.Dtos
{
    public class QuizValidationResultDto
    {

        public bool IsValid { get; set; }

        public List<string> Errors { get; set; }

        public List<string> Warnings { get; set; }

        public List<string> SuccessMessages { get; set; }

        public int TotalQuestions { get; set; }

        public int TotalScore { get; set; }
    }
}