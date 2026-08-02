using QuizPlatform.API.Models.DTO;
using QuizPlatform.API.Models.Dtos;
using QuizPlatform.API.Models.Entity;
using System.Collections.Generic;


namespace QuizPlatform.API.Services.Interface
{
    public interface IQuizService
    {
        string CekKoneksiDB();

        List<Quiz> GetAllQuizzes();

        Quiz CreateQuiz(Quiz quiz);

        List<Quiz> FilterQuizzes(int categoryId, int difficultyId);

        List<Quiz> GetTeacherQuizzes(int teacherId);

        void PublishQuiz(int quizId);

        void UnpublishQuiz(int quizId);

        void DeleteQuiz(int quizId);

        void UpdateQuiz(Quiz quiz);

        Quiz GetQuizById(int quizId);

               
        QuizValidationResultDto ValidateQuizBeforePublish(int quizId);



    }

}