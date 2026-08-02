using QuizPlatform.API.Models.DTO;
using QuizPlatform.API.Models.Dtos;
using QuizPlatform.API.Models.Entity;
using System.Collections.Generic;

namespace QuizPlatform.API.Services.Interface
{
    public interface IQuizAttemptService
    {
        QuizAttempt StartQuiz(QuizAttempt attempt);

        void SubmitAnswer(UserAnswer answer);

        void EndQuiz(int attemptId);

        void CreateLeaderboard(int attemptId);

        void UpdateRanking(int quizId);

        List<LeaderboardDto> GetLeaderboard(int quizId);

        QuizResultDto GetQuizResult(int attemptId);

        List<QuizHistoryDto> GetStudentQuizHistory(int userId);
    }
}