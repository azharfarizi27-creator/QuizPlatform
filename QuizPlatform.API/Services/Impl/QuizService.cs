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

        public Quiz CreateQuiz(Quiz quiz)
        {
            quiz.CreatedAt = DateTime.Now;
            quiz.Status = "Draft";
            context.Quizzes.Add(quiz);

            context.SaveChanges();

            return quiz;
        }
        public List<Quiz> GetAllQuizzes()
        {
            return context.Quizzes.ToList();
        }

        public List<Quiz> GetTeacherQuizzes(int teacherId)
        {
            return context.Quizzes
                .Where(x => x.CreatedBy == teacherId)
                .OrderByDescending(x => x.CreatedAt)
                .ToList();
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
            if (option.IsCorrect)
            {
                var existingCorrect =
                    context.QuestionOptions
                    .FirstOrDefault(x =>
                        x.QuestionId == option.QuestionId &&
                        x.IsCorrect == true);

                if (existingCorrect != null)
                {
                    throw new Exception(
                        "Sudah ada jawaban benar untuk soal ini"
                    );
                }
            }

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
            var question = context.Questions
                .FirstOrDefault(x => x.Id == option.QuestionId);

            answer.EarnedScore =
                option.IsCorrect ? question.Score : 0; answer.AnsweredAt = DateTime.Now;

            context.UserAnswers.Add(answer);

            // 🔥 UPDATE SCORE
            if (option.IsCorrect)
            {
                attempt.TotalScore += question.Score;
            }

            context.SaveChanges();
        }
        public QuizAttempt StartQuiz(QuizAttempt attempt)
        {
            var finishedAttempt =
                context.QuizAttempts
                    .FirstOrDefault(x =>
                        x.UserId == attempt.UserId &&
                        x.QuizId == attempt.QuizId &&
                        x.Status == false
                    );

            if (finishedAttempt != null)
            {
                throw new Exception("Quiz sudah pernah dikerjakan");
            }

            var runningAttempt =
                context.QuizAttempts
                    .FirstOrDefault(x =>
                        x.UserId == attempt.UserId &&
                        x.QuizId == attempt.QuizId &&
                        x.Status == true
                    );

            if (runningAttempt != null)
            {
                return runningAttempt;
            }

            attempt.StartTime = DateTime.Now;
            attempt.EndTime = null;
            attempt.TotalScore = 0;
            attempt.Status = true;

            context.QuizAttempts.Add(attempt);
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
            {
                existing.Score = attempt.TotalScore;
                existing.DurationInSeconds =
                    (int)(attempt.EndTime.Value - attempt.StartTime).TotalSeconds;
                existing.CreatedAt = DateTime.Now;

                context.SaveChanges();

                UpdateRanking(attempt.QuizId);

                return;
            }
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

        public List<QuizHistoryDto> GetStudentQuizHistory(int userId)
        {
            var result =
                context.QuizAttempts
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.StartTime)
                .Select(x => new QuizHistoryDto
                {
                    AttemptId = x.Id,
                    QuizId = x.QuizId,
                    QuizTitle =
                        context.Quizzes
                        .Where(q => q.Id == x.QuizId)
                        .Select(q => q.Title)
                        .FirstOrDefault(),

                    StartTime = x.StartTime,
                    EndTime = x.EndTime,
                    TotalScore = x.TotalScore,
                    Status = x.Status
                })
                .ToList();

            return result;
        }

        public List<TeacherQuizResultDto>
    GetTeacherAnalytics()
        {
            var data =
                context.QuizAttempts
                .Where(x => x.EndTime != null)
                .ToList();

            var result =
                data.Select(x =>
                {
                    var answers =
                        context.UserAnswers
                        .Where(a =>
                            a.AttemptId == x.Id)
                        .ToList();

                    return new TeacherQuizResultDto
                    {
                        AttemptId = x.Id,

                        StudentName =
                            context.Users
                            .Where(u => u.Id == x.UserId)
                            .Select(u => u.FullName)
                            .FirstOrDefault(),

                        QuizTitle =
                            context.Quizzes
                            .Where(q => q.Id == x.QuizId)
                            .Select(q => q.Title)
                            .FirstOrDefault(),

                        Score = x.TotalScore,

                        TotalCorrect =
                            answers.Count(a =>
                                a.IsCorrect == true),

                        TotalWrong =
                            answers.Count(a =>
                                a.IsCorrect == false),

                        DurationInSeconds =
                            x.EndTime.HasValue
                            ? (int)(x.EndTime.Value -
                                x.StartTime).TotalSeconds
                            : 0,

                        StartTime =
                            x.StartTime,

                        EndTime =
                            x.EndTime.Value
                    };
                }).ToList();

            return result;
        }

        public DashboardStatsDto GetDashboardStats()
        {
            return new DashboardStatsDto
            {
                TotalUsers =
                    context.Users.Count(),

                TotalStudents =
                    context.Users.Count(x =>
                        x.Role.Name == "Student"
                    ),

                TotalTeachers =
                    context.Users.Count(x =>
                        x.Role.Name == "Teacher"
                    ),

                TotalQuizzes =
                    context.Quizzes.Count(),

                TotalQuestions =
                    context.Questions.Count(),

                TotalAttempts =
                    context.QuizAttempts.Count()
            };
        }

        public List<TopStudentDto> GetTopStudents()
        {
            var data =
                context.QuizAttempts
                .Where(x => x.EndTime != null)
                .GroupBy(x => x.UserId)
                .Select(g => new TopStudentDto
                {
                    UserId = g.Key,

                    StudentName =
                        context.Users
                        .Where(u => u.Id == g.Key)
                        .Select(u => u.FullName)
                        .FirstOrDefault(),

                    TotalAttempts =
                        g.Count(),

                    HighestScore =
                        g.Max(x => x.TotalScore),

                    AverageScore =
                        g.Average(x => x.TotalScore)
                })
                .OrderByDescending(x => x.HighestScore)
                .ThenByDescending(x => x.AverageScore)
                .Take(10)
                .ToList();

            return data;
        }
        public void RegisterStudent(User user)
        {
            var existingUser = context.Users
                .FirstOrDefault(x =>
                    x.Username == user.Username ||
                    x.Email == user.Email);

            if (existingUser != null)
                throw new Exception("Username atau Email sudah digunakan");

            user.PasswordHash =
                PasswordGenerator.GenerateHash(user.PasswordHash);

            user.RoleId = 3; // Student
            user.IsActive = true;
            user.CreatedAt = DateTime.Now;

            context.Users.Add(user);
            context.SaveChanges();
        }

        public List<Quiz> FilterQuizzes(int categoryId, int difficultyId)
        {
            return context.Quizzes
                .Where(x =>
                    x.CategoryId == categoryId &&
                    x.DifficultyId == difficultyId &&
                    x.Status == "Active"
                )
                .ToList();
        }

        public void DeleteQuestionOption(int optionId)
        {
            var option = context.QuestionOptions
                .FirstOrDefault(x => x.Id == optionId);

            if (option == null)
                throw new Exception("Jawaban tidak ditemukan");

            context.QuestionOptions.Remove(option);
            context.SaveChanges();
        }

        public void UpdateQuestionOption(
    QuestionOption option)
        {
            var existing =
                context.QuestionOptions
                .FirstOrDefault(x =>
                    x.Id == option.Id);

            if (existing == null)
                throw new Exception(
                    "Jawaban tidak ditemukan"
                );

            existing.OptionText =
                option.OptionText;

            existing.OrderNumber =
                option.OrderNumber;

            existing.IsCorrect =
                option.IsCorrect;

            context.SaveChanges();
        }

        public void UpdateQuestion(Question question)
        {
            var existing = context.Questions
                .FirstOrDefault(x => x.Id == question.Id);

            if (existing == null)
                throw new Exception("Soal tidak ditemukan");

            existing.QuestionText = question.QuestionText;
            existing.Score = question.Score;
            existing.OrderNumber = question.OrderNumber;
            existing.Explanation = question.Explanation;
            existing.QuestionImage = question.QuestionImage;

            context.SaveChanges();
        }

        public void DeleteQuestion(int questionId)
        {
            var question = context.Questions
                .FirstOrDefault(x => x.Id == questionId);

            if (question == null)
                throw new Exception("Soal tidak ditemukan");

            var options = context.QuestionOptions
                .Where(x => x.QuestionId == questionId)
                .ToList();

            context.QuestionOptions.RemoveRange(options);
            context.Questions.Remove(question);

            context.SaveChanges();
        }


        public void PublishQuiz(int quizId)
        {
            var quiz = context.Quizzes
                .FirstOrDefault(x => x.Id == quizId);

            if (quiz == null)
                throw new Exception("Quiz tidak ditemukan");

            var questions = context.Questions
                .Where(x => x.QuizId == quizId)
                .ToList();

            if (questions.Count == 0)
                throw new Exception("Quiz belum punya soal");

            foreach (var question in questions)
            {
                var options = context.QuestionOptions
                    .Where(x => x.QuestionId == question.Id)
                    .ToList();

                if (options.Count < 2)
                    throw new Exception("Setiap soal minimal punya 2 jawaban");

                var hasCorrectAnswer = options
                    .Any(x => x.IsCorrect == true);

                if (!hasCorrectAnswer)
                    throw new Exception("Setiap soal harus punya jawaban benar");
            }

            quiz.Status = "Active";
            quiz.UpdatedAt = DateTime.Now;

            context.SaveChanges();
        }

        public void UnpublishQuiz(int quizId)
        {
            var quiz = context.Quizzes
                .FirstOrDefault(x => x.Id == quizId);

            if (quiz == null)
                throw new Exception("Quiz tidak ditemukan");

            quiz.Status = "Draft";
            quiz.UpdatedAt = DateTime.Now;

            context.SaveChanges();
        }

        public void DeleteQuiz(int quizId)
        {
            var quiz = context.Quizzes
                .FirstOrDefault(x => x.Id == quizId);

            if (quiz == null)
                throw new Exception("Quiz tidak ditemukan");

            var hasAttempt = context.QuizAttempts
                .Any(x => x.QuizId == quizId);

            if (hasAttempt)
                throw new Exception(
                    "Quiz tidak bisa dihapus karena sudah pernah dikerjakan student. Gunakan Unpublish saja."
                );

            var questions = context.Questions
                .Where(x => x.QuizId == quizId)
                .ToList();

            foreach (var question in questions)
            {
                var options = context.QuestionOptions
                    .Where(x => x.QuestionId == question.Id)
                    .ToList();

                context.QuestionOptions.RemoveRange(options);
            }

            context.Questions.RemoveRange(questions);
            context.Quizzes.Remove(quiz);

            context.SaveChanges();
        }
        public void UpdateQuiz(Quiz quiz)
        {
            var existing = context.Quizzes
                .FirstOrDefault(x => x.Id == quiz.Id);

            if (existing == null)
                throw new Exception("Quiz tidak ditemukan");

            existing.Title = quiz.Title;
            existing.Description = quiz.Description;
            existing.CategoryId = quiz.CategoryId;
            existing.DifficultyId = quiz.DifficultyId;
            existing.LevelId = quiz.LevelId;
            existing.DurationInMinutes = quiz.DurationInMinutes;
            existing.PassingScore = quiz.PassingScore;
            existing.Thumbnail = quiz.Thumbnail;
            existing.UpdatedAt = DateTime.Now;

            context.SaveChanges();
        }
        public Quiz GetQuizById(int quizId)
        {
            return context.Quizzes
                .FirstOrDefault(x => x.Id == quizId);
        }

        public TeacherStatsSummaryDto GetTeacherStatsSummary()
        {
            var attempts = context.QuizAttempts
                .Where(x => x.EndTime != null)
                .ToList();

            int passedCount = 0;
            int failedCount = 0;

            foreach(var attempt in attempts)
            {
                var passingScore = context.Quizzes
                    .Where(q => q.Id == attempt.QuizId)
                    .Select(q => q.PassingScore)
                    .FirstOrDefault();
                if (attempt.TotalScore >= passingScore)
                    passedCount++;
                else
                    failedCount++;
            }

            var total = passedCount + failedCount;

            return new TeacherStatsSummaryDto
            {
                AverageQuizScore = attempts.Any() ? attempts.Average(x => x.TotalScore) : 0,
                PassedCount = passedCount,
                FailedCount = failedCount,
                PassRate = total > 0 ? ((double)passedCount / total) * 100 : 0
            };
        }

        public List<AdminUserDto> GetAdminUsers()
        {
            var users =
                context.Users
                .Include("Role")
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new AdminUserDto
                {
                    Id = x.Id,
                    FullName = x.FullName,
                    Username = x.Username,
                    Email = x.Email,
                    RoleId = x.RoleId,
                    RoleName = x.Role.Name,
                    IsActive = x.IsActive,
                    CreatedAt = x.CreatedAt
                })
                .ToList();

            return users;
        }

        public void ChangeUserRole(ChangeUserRoleDto request)
        {
            var user =
                context.Users
                .FirstOrDefault(x => x.Id == request.UserId);

            if (user == null)
                throw new Exception("User tidak ditemukan");

            var role =
                context.Roles
                .FirstOrDefault(x => x.Id == request.RoleId);

            if (role == null)
                throw new Exception("Role tidak ditemukan");

            user.RoleId = request.RoleId;

            context.SaveChanges();
        }

        public void DeleteUser(int userId)
        {
            var user =
                context.Users
                .FirstOrDefault(x => x.Id == userId);

            if (user == null)
                throw new Exception("User tidak ditemukan");

            user.IsActive = false;

            context.SaveChanges();
        }

        public void ActivateUser(int userId)
        {
            var user =
                context.Users
                .FirstOrDefault(x => x.Id == userId);

            if (user == null)
                throw new Exception("User tidak ditemukan");

            user.IsActive = true;

            context.SaveChanges();
        }

        public List<Role> GetAllRoles()
        {
            return context.Roles
                .OrderBy(x => x.Id)
                .ToList();
        }

        public void CreateDifficulty(Difficulty difficulty)
        {
            context.Difficulties.Add(difficulty);

            context.SaveChanges();
        }


        public StudentProfileStatsDto GetStudentProfileStats(
    int userId
)
        {
            var user =
                context.Users
                    .FirstOrDefault(
                        x => x.Id == userId
                    );

            if (user == null)
                throw new Exception(
                    "User tidak ditemukan"
                );

            var attempts =
                context.QuizAttempts
                    .Where(x =>
                        x.UserId == userId &&
                        x.EndTime != null
                    )
                    .ToList();

            var passed =
                attempts.Count(x =>
                    x.TotalScore >= 75
                );

            return new StudentProfileStatsDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Username = user.Username,
                Email = user.Email,
                ProfileImage = user.ProfileImage,

                TotalQuiz = attempts.Count,

                PassedQuiz = passed,

                FailedQuiz =
                    attempts.Count - passed,

                HighestScore =
                    attempts.Any()
                        ? attempts.Max(x => x.TotalScore)
                        : 0,

                AverageScore =
                    attempts.Any()
                        ? attempts.Average(x => x.TotalScore)
                        : 0
            };
        }

        public void UpdateProfile(
    int userId,
    UpdateProfileDto request
)
        {
            var user =
                context.Users
                    .FirstOrDefault(
                        x => x.Id == userId
                    );

            if (user == null)
                throw new Exception(
                    "User tidak ditemukan"
                );

            user.FullName =
                request.FullName;

            user.Username =
                request.Username;

            user.Email =
                request.Email;

            user.UpdatedAt =
                DateTime.Now;

            context.SaveChanges();
        }

        public void ChangePassword(
            int userId,
            ChangePasswordDto request
        )
        {
            var user =
                context.Users
                    .FirstOrDefault(x => x.Id == userId);

            if (user == null)
                throw new Exception("User tidak ditemukan");

            string oldPasswordHash =
                PasswordGenerator.GenerateHash(
                    request.OldPassword
                );

            if (user.PasswordHash != oldPasswordHash)
            {
                throw new Exception("Password lama salah");
            }

            string newPasswordHash =
                PasswordGenerator.GenerateHash(
                    request.NewPassword
                );

            user.PasswordHash = newPasswordHash;
            user.UpdatedAt = DateTime.Now;

            context.SaveChanges();
        }

        public void UpdateProfileImage(
    int userId,
    string profileImage
)
        {
            var user =
                context.Users
                    .FirstOrDefault(
                        x => x.Id == userId
                    );

            if (user == null)
                throw new Exception(
                    "User tidak ditemukan"
                );

            user.ProfileImage =
                profileImage;

            user.UpdatedAt =
                DateTime.Now;

            context.SaveChanges();
        }



        public List<StudentNotificationDto> GetStudentNotifications(int userId)
        {
           var attemptdQuizIds = context.QuizAttempts
                .Where(x => x.UserId == userId)
                .Select(x => x.QuizId)
                .ToList();

            var quizzes = context.Quizzes
                .Where(x => attemptdQuizIds.Contains(x.Id)
                )
                .OrderByDescending(x => x.CreatedAt)
                .Take(5)
                .ToList();

            return quizzes.Select(x => new StudentNotificationDto
            {
                QuizId = x.Id,
                QuizTitle = x.Title,
                Message = "Quiz baru tersedia: " + x.Title,
                CreatedAt = x.CreatedAt
            }).ToList();



           
        }
    }
}