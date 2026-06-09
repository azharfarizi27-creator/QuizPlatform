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

        Quiz CreateQuiz(Quiz quiz);

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

        List<TeacherQuizResultDto>
    GetTeacherAnalytics();

        DashboardStatsDto
GetDashboardStats();
        void RegisterStudent(User user);

        List<Quiz> FilterQuizzes(int categoryId, int difficultyId);

        List<Quiz> GetTeacherQuizzes(int teacherId);

        void DeleteQuestionOption(int optionId);

        void UpdateQuestionOption(
    QuestionOption option
);
        void UpdateQuestion(Question question);
        void DeleteQuestion(int questionId);

        void PublishQuiz(int quizId);

        void UnpublishQuiz(int quizId);

        void DeleteQuiz(int quizId);

        void UpdateQuiz(Quiz quiz);

        Quiz GetQuizById(int quizId);

        List<QuizHistoryDto> GetStudentQuizHistory(int userId);

        List<TopStudentDto> GetTopStudents();

        StudentProfileStatsDto GetStudentProfileStats(int userId);

        TeacherStatsSummaryDto GetTeacherStatsSummary();

        List<AdminUserDto> GetAdminUsers();

        void ChangeUserRole(ChangeUserRoleDto request);

        void DeleteUser(int userId);

        void ActivateUser(int userId);

        List<Role> GetAllRoles();

        void CreateDifficulty(Difficulty difficulty);

      

        void UpdateProfile(
            int userId,
            UpdateProfileDto request
        );

        void ChangePassword(
            int userId,
            ChangePasswordDto request
        );

        void UpdateProfileImage( int userId, string profileImage);

        List<StudentNotificationDto> GetStudentNotifications(int userId);


    }

}