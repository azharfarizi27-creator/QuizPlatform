using QuizPlatform.API.Models.DTO;
using QuizPlatform.API.Models.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuizPlatform.API.Services.Interface
{
    public interface IQuizService
    {
        string CekKoneksiDB();
        List<User> GetAllUser();

        void CreateUser(User user);

        //User Login(string username, string password);
        object Login(string username, string password);


        List<Category> GetAllCategories();

        void CreateCategory(Category category);

        List<Level> GetAllLevels();

        void CreateLevel(Level level);

        List<Difficulty> GetAllDifficulties();

        List<Quiz> GetAllQuizzes();

        void CreateQuiz(Quiz quiz);

        List<Question> GetAllQuestions();

        void CreateQuestion(Question question);

        List<QuestionOption> GetAllQuestionOptions();

        void CreateQuestionOption(QuestionOption option);

        void SubmitAnswer(UserAnswer answer);

        QuizAttempt StartQuiz(QuizAttempt attempt);

        void CreateLeaderboard(int attemptId);

        void EndQuiz(int attemptId);

        List<Leaderboard> GetLeaderboard(int quizId);
        void UpdateRanking(int quizId);

        QuizResultDto GetQuizResult(int attemptId);
    }

}