using QuizPlatform.API.Helpers;
using QuizPlatform.API.Models.DTO;
using QuizPlatform.API.Models.Entity;
using QuizPlatform.API.Models.Generator;
using QuizPlatform.API.Services.Base;
using QuizPlatform.API.Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuizPlatform.API.Services.Impl
{
    public class QuizService : BaseServices, IQuizService
    {

        public QuizService()
           : base()
        {
        }

        public string CekKoneksiDB()
        {
            string result = "";

            try
            {
                if (context.Database.Exists())
                {
                    result = "Koneksi Berhasil! ";
                }
                else
                {
                    return "Database tidak ditemukan.";
                }

                var query = "SELECT FORMAT(GETDATE(),'yyyy-MM-dd HH:mm:ss')";
                var timeFromDB = context.Database
                    .SqlQuery<string>(query)
                    .FirstOrDefault();

                result += timeFromDB;

                return result;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        

        public List<User> GetAllUser()
        {
            return context.Users.ToList();
        }


        public void CreateUser(User user)
        {
            user.PasswordHash =
                PasswordGenerator.GenerateHash(
                    user.PasswordHash
                );

            user.CreatedAt = DateTime.Now;

            context.Users.Add(user);

            context.SaveChanges();
        }
        public object Login(string username, string password)
        {
            string hashed = PasswordGenerator.GenerateHash(password);

            var user = context.Users
                .Include("Role") // 🔥 penting biar Role tidak null
                .FirstOrDefault(x =>
                    x.Username == username &&
                    x.PasswordHash == hashed &&
                    x.IsActive == true
                );

            if (user == null)
                return null;

            return new
            {
                user.Id,
                user.FullName,
                user.Username,
                user.Email,
                user.RoleId,
                RoleName = user.Role?.Name,

                Token = JwtHelper.GenerateToken(user.Id, user.Role.Name)
            };
        }
        public List<Category> GetAllCategories()
        {
            return context.Categories.ToList();
        }

        public void CreateCategory(Category category)
        {
            category.CreatedAt = DateTime.Now;

            context.Categories.Add(category);

            context.SaveChanges();
        }

        public List<Level> GetAllLevels()
        {
            return context.Levels.ToList();
        }

        public void CreateLevel(Level level)
        {

            context.Levels.Add(level);

            context.SaveChanges();
        }

        public List<Difficulty> GetAllDifficulties()
        {
            return context.Difficulties.ToList();
        }

        public void CreateQuiz(Quiz quiz)
        {
            quiz.CreatedAt = DateTime.Now;

            context.Quizzes.Add(quiz);

            context.SaveChanges();
        }
        public List<Quiz> GetAllQuizzes()
        {
            return context.Quizzes.ToList();
        }

        public List<Question> GetAllQuestions()
        {
            return context.Questions.ToList();
        }

        public void CreateQuestion(Question question)
        {
            question.CreatedAt = DateTime.Now;

            context.Questions.Add(question);

            context.SaveChanges();
        }

        public List<QuestionOption> GetAllQuestionOptions()
        {
            return context.QuestionOptions.ToList();
        }

        public void CreateQuestionOption(
            QuestionOption option)
        {
            option.CreatedAt = DateTime.Now;

            context.QuestionOptions.Add(option);

            context.SaveChanges();
        }

        public void SubmitAnswer(UserAnswer answer)
        {
            // 🔥 CEK ATTEMPT DULU
            var attempt = context.QuizAttempts
                .FirstOrDefault(x => x.Id == answer.AttemptId);

            if (attempt == null)
                throw new Exception("Attempt tidak ditemukan");

            // 🔒 INI LOCK UTAMA
            if (attempt.Status == false)
                throw new Exception("Quiz sudah selesai, tidak bisa menjawab lagi");

            var option = context.QuestionOptions
                .FirstOrDefault(x => x.Id == answer.QuestionOptionId);

            if (option == null)
                throw new Exception("QuestionOption tidak ditemukan");

            answer.QuestionId = option.QuestionId;
            answer.IsCorrect = option.IsCorrect;
            answer.EarnedScore = option.IsCorrect ? 1 : 0;
            answer.AnsweredAt = DateTime.Now;

            context.UserAnswers.Add(answer);

            // 🔥 UPDATE SCORE
            if (option.IsCorrect)
            {
                attempt.TotalScore += 1;
            }

            context.SaveChanges();
        }
        public QuizAttempt StartQuiz(QuizAttempt attempt)
        {
            attempt.StartTime = DateTime.Now;

            attempt.TotalScore = 0;

            attempt.Status = true;

            context.QuizAttempts.Add(
                attempt
            );

            context.SaveChanges();

            return attempt;
        }

        public void CreateLeaderboard(int attemptId)
        {
            var attempt = context.QuizAttempts
                .FirstOrDefault(x => x.Id == attemptId);

            if (attempt == null)
                throw new Exception("Attempt tidak ditemukan");

            if (attempt.EndTime == null)
                throw new Exception("Quiz belum selesai");

            var existing = context.Leaderboards
                .FirstOrDefault(x => x.UserId == attempt.UserId && x.QuizId == attempt.QuizId);

            if (existing != null)
                throw new Exception("Leaderboard sudah dibuat");

            var leaderboard = new Leaderboard
            {
                UserId = attempt.UserId,
                QuizId = attempt.QuizId,
                Score = attempt.TotalScore,
                DurationInSeconds = (int)(attempt.EndTime.Value - attempt.StartTime).TotalSeconds,
                RankPosition = 0,
                CreatedAt = DateTime.Now
            };

            context.Leaderboards.Add(leaderboard);
            context.SaveChanges();

            // 🔥 INI YANG KURANG SEBELUMNYA
            UpdateRanking(attempt.QuizId);
        }
        public void EndQuiz(int attemptId)
        {
            var attempt = context.QuizAttempts
                .FirstOrDefault(x => x.Id == attemptId);

            if (attempt == null)
                throw new Exception(
                    "Attempt tidak ditemukan"
                );

            // quiz selesai
            attempt.EndTime =
                DateTime.Now;

            attempt.Status =
                false;

            context.SaveChanges();

            // otomatis masuk leaderboard
            CreateLeaderboard(
                attemptId
            );

            // update ranking
            UpdateRanking(
                attempt.QuizId
            );
        }
        public List<Leaderboard> GetLeaderboard(int quizId)
        {
            return context.Leaderboards
                .Where(x => x.QuizId == quizId)
                .OrderByDescending(x => x.Score)
                .ThenBy(x => x.DurationInSeconds)
                .ToList();
        }

        public void UpdateRanking(int quizId)
        {
            var list = context.Leaderboards
                .Where(x => x.QuizId == quizId)
                .OrderByDescending(x => x.Score)
                .ThenBy(x => x.DurationInSeconds)
                .ToList();

            int rank = 1;

            foreach (var item in list)
            {
                item.RankPosition = rank;
                rank++;
            }

            context.SaveChanges();
        }

        public QuizResultDto GetQuizResult(int attemptId)
        {
            var attempt = context.QuizAttempts
                .FirstOrDefault(x => x.Id == attemptId);

            if (attempt == null)
                throw new Exception("Attempt tidak ditemukan");

            var answers = context.UserAnswers
                .Where(x => x.AttemptId == attemptId)
                .ToList();

            var data = (from ua in context.UserAnswers
                        join q in context.Questions on ua.QuestionId equals q.Id
                        where ua.AttemptId == attemptId
                        select new { ua, q }).ToList();

            var correctAnswers = context.QuestionOptions
                .Where(x => x.IsCorrect)
                .ToList();

            var details = data.Select(x => new QuizResultDetailDto
            {
                QuestionId = x.q.Id,
                QuestionText = x.q.QuestionText,

                UserAnswer = context.QuestionOptions
                    .Where(o => o.Id == x.ua.QuestionOptionId)
                    .Select(o => o.OptionText)
                    .FirstOrDefault() ?? "",

                CorrectAnswer = context.QuestionOptions
                    .Where(o => o.QuestionId == x.q.Id && o.IsCorrect)
                    .Select(o => o.OptionText)
                    .FirstOrDefault() ?? "",

                IsCorrect = x.ua.IsCorrect == true,
                Score = (x.ua.IsCorrect == true) ? x.q.Score : 0
            }).ToList();

            var result = new QuizResultDto
            {
                AttemptId = attempt.Id,
                UserId = attempt.UserId,
                QuizId = attempt.QuizId,
                TotalScore = attempt.TotalScore,

                TotalCorrect = answers.Count(x => x.IsCorrect == true),
                TotalWrong = answers.Count(x => x.IsCorrect == false),

                StartTime = attempt.StartTime,
                EndTime = attempt.EndTime ?? DateTime.Now,

                DurationInSeconds = attempt.EndTime.HasValue
                    ? (int)(attempt.EndTime.Value - attempt.StartTime).TotalSeconds
                    : 0,

                Details = details
            };

            return result;
        }


    }
}