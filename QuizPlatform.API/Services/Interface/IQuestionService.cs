using QuizPlatform.API.Models.Dtos;
using QuizPlatform.API.Models.Entity;
using System.Collections.Generic;

namespace QuizPlatform.API.Services.Interface
{
    public interface IQuestionService
    {
        List<Question> GetAllQuestions();

        void CreateQuestion(Question question);

        void UpdateQuestion(Question question);

        void DeleteQuestion(int questionId);

        List<QuestionOption> GetAllQuestionOptions();

        void CreateQuestionOption(QuestionOption option);

        void UpdateQuestionOption(QuestionOption option);

        void DeleteQuestionOption(int optionId);

        List<Question> GetQuestionsByAttempt(int attemptId);

        List<QuestionOption> GetOptionsByAttemptQuestion(
            int attemptId,
            int questionId
        );

        int ImportQuestionsFromExcel(ImportQuestionExcelRequestDto request);
    }
}