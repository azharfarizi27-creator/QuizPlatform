using QuizPlatform.API.Models.DTO;
using QuizPlatform.API.Models.Dtos;
using QuizPlatform.API.Models.Entity;
using System.Collections.Generic;

namespace QuizPlatform.API.Services.Interface
{
    public interface IQuestionBankService
    {
        List<QuestionBank> GetQuestionBanks();

        QuestionBank GetQuestionBankById(int id);

        QuestionBank CreateQuestionBank(QuestionBank bank);

        void UpdateQuestionBank(QuestionBank bank);

        void DeleteQuestionBank(int id);

        List<Question> GetQuestionsByBank(int bankId);

        Question CopyQuestionToQuiz(int questionId, int quizId);

        int CopyRandomQuestionsFromBankToQuiz(
            CopyRandomQuestionDto request
        );
    }
}