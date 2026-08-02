using System.Collections.Generic;

namespace QuizPlatform.API.Models.Dtos
{
    public class ImportQuestionExcelRequestDto
    {
        public int QuizId { get; set; }

        public List<ImportQuestionExcelRowDto> Questions { get; set; }
    }

    public class ImportQuestionExcelRowDto
    {
        public string QuestionText { get; set; }

        public string OptionA { get; set; }

        public string OptionB { get; set; }

        public string OptionC { get; set; }

        public string OptionD { get; set; }

        public string CorrectAnswer { get; set; }

        public int Score { get; set; }

        public string Explanation { get; set; }

        public int? QuestionBankId { get; set; }
    }
}