using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuizPlatform.API.Models.Dtos
{
    public class CopyRandomQuestionDto
    {
        public int QuestionBankId { get; set; }

        public int QuizId { get; set; }

        public int TotalQuestion { get; set; }
    }
}