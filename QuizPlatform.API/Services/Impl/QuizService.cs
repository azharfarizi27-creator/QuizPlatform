using QuizPlatform.API.Models.DTO;
using QuizPlatform.API.Models.Dtos;
using QuizPlatform.API.Models.Entity;
using QuizPlatform.API.Services.Base;
using QuizPlatform.API.Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QuizPlatform.API.Services.Impl
{
    public class QuizService : BaseServices, IQuizService
    {
        private readonly IActivityLogService activityLogService =
            new ActivityLogService();
        
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


        public Quiz CreateQuiz(Quiz quiz)
        {
            quiz.CreatedAt = DateTime.Now;
            quiz.Status = "Draft";

            if (quiz.MaxAttempt < 0)
                quiz.MaxAttempt = 0;

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



        public List<Quiz> FilterQuizzes(int categoryId, int difficultyId )
        {
            return context.Quizzes
                .Where(x =>
                    x.CategoryId == categoryId &&
                    x.DifficultyId == difficultyId &&
                    (
                        x.Status == "Active" ||
                        x.Status == "Published"
                    )
                )
                .ToList();
        }


        public void PublishQuiz(int quizId)
        {
            var validation =
                ValidateQuizBeforePublish(quizId);

            if (!validation.IsValid)
            {
                throw new Exception(
                    "Quiz tidak bisa dipublish:\n" +
                    string.Join("\n", validation.Errors)
                );
            }

            var quiz =
                context.Quizzes
                    .FirstOrDefault(x => x.Id == quizId);

            if (quiz == null)
                throw new Exception("Quiz tidak ditemukan");

            quiz.Status =
                "Published";

            quiz.UpdatedAt =
                DateTime.Now;

            context.SaveChanges();

            activityLogService.CreateActivityLog(
                quiz.CreatedBy,
                "PUBLISH_QUIZ",
                "Teacher/Admin publish quiz: " + quiz.Title
            );
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

            existing.MaxAttempt = quiz.MaxAttempt;

            existing.StartDate = quiz.StartDate;
            existing.EndDate = quiz.EndDate;

            existing.Thumbnail = quiz.Thumbnail;
            existing.UpdatedAt = DateTime.Now;

            context.SaveChanges();
        }

        public Quiz GetQuizById(int quizId)
        {
            return context.Quizzes
                .FirstOrDefault(x => x.Id == quizId);
        }

        public QuizValidationResultDto ValidateQuizBeforePublish(int quizId)
        {
            var result =
                new QuizValidationResultDto
                {
                    IsValid = true,
                    Errors = new List<string>(),
                    Warnings = new List<string>(),
                    SuccessMessages = new List<string>()
                };

            var quiz =
                context.Quizzes
                    .FirstOrDefault(x =>
                        x.Id == quizId
                    );

            if (quiz == null)
                throw new Exception("Quiz tidak ditemukan");

            if (string.IsNullOrWhiteSpace(quiz.Title))
            {
                result.Errors.Add("Judul quiz wajib diisi");
            }
            else
            {
                result.SuccessMessages.Add("Judul quiz sudah diisi");
            }

            if (quiz.DurationInMinutes <= 0)
            {
                result.Errors.Add("Durasi quiz harus lebih dari 0 menit");
            }
            else
            {
                result.SuccessMessages.Add("Durasi quiz sudah valid");
            }

            if (quiz.PassingScore < 0)
            {
                result.Errors.Add("Passing score tidak boleh kurang dari 0");
            }
            else
            {
                result.SuccessMessages.Add("Passing score sudah valid");
            }

            if (
                quiz.StartDate.HasValue &&
                quiz.EndDate.HasValue &&
                quiz.StartDate.Value >= quiz.EndDate.Value
            )
            {
                result.Errors.Add("Tanggal mulai harus lebih kecil dari tanggal selesai");
            }
            else
            {
                result.SuccessMessages.Add("Jadwal quiz sudah valid");
            }

            var questions =
                context.Questions
                    .Where(x =>
                        x.QuizId == quizId
                    )
                    .OrderBy(x =>
                        x.OrderNumber
                    )
                    .ToList();

            if (questions.Count == 0)
            {
                result.Errors.Add("Quiz harus memiliki minimal 1 soal");
            }
            else
            {
                result.SuccessMessages.Add("Quiz sudah memiliki " + questions.Count + " soal");
            }

            foreach (var question in questions)
            {
                var questionLabel =
                    question.OrderNumber > 0
                        ? "Soal urutan " + question.OrderNumber
                        : "Soal ID " + question.Id;

                var questionHasError =
                    false;

                if (string.IsNullOrWhiteSpace(question.QuestionText))
                {
                    result.Errors.Add(questionLabel + ": pertanyaan wajib diisi");
                    questionHasError = true;
                }

                if (question.Score <= 0)
                {
                    result.Errors.Add(questionLabel + ": score harus lebih dari 0");
                    questionHasError = true;
                }

                if (question.QuestionTypeId <= 0)
                {
                    result.Errors.Add(questionLabel + ": tipe soal wajib dipilih");
                    questionHasError = true;
                    continue;
                }

                if (question.QuestionTypeId == 1)
                {
                    var options =
                        context.QuestionOptions
                            .Where(x =>
                                x.QuestionId == question.Id
                            )
                            .ToList();

                    if (options.Count < 2)
                    {
                        result.Errors.Add(
                            questionLabel +
                            ": soal multiple choice harus memiliki minimal 2 opsi jawaban"
                        );

                        questionHasError = true;
                    }

                    var correctAnswerCount =
                        options.Count(x =>
                            x.IsCorrect == true
                        );

                    if (correctAnswerCount == 0)
                    {
                        result.Errors.Add(
                            questionLabel +
                            ": soal multiple choice harus memiliki minimal 1 jawaban benar"
                        );

                        questionHasError = true;
                    }

                    if (correctAnswerCount > 1)
                    {
                        result.Warnings.Add(
                            questionLabel +
                            ": memiliki lebih dari 1 jawaban benar"
                        );
                    }

                    var hasEmptyOption =
                        options.Any(x =>
                            string.IsNullOrWhiteSpace(x.OptionText)
                        );

                    if (hasEmptyOption)
                    {
                        result.Errors.Add(
                            questionLabel +
                            ": terdapat opsi jawaban yang masih kosong"
                        );

                        questionHasError = true;
                    }

                    if (!questionHasError)
                    {
                        result.SuccessMessages.Add(
                            questionLabel +
                            ": soal multiple choice sudah valid"
                        );
                    }
                }
                else if (question.QuestionTypeId == 3)
                {
                    var hasOptions =
                        context.QuestionOptions
                            .Any(x =>
                                x.QuestionId == question.Id
                            );

                    if (hasOptions)
                    {
                        result.Warnings.Add(
                            questionLabel +
                            ": soal essay tidak membutuhkan opsi jawaban"
                        );
                    }

                    if (string.IsNullOrWhiteSpace(question.Explanation))
                    {
                        result.Warnings.Add(
                            questionLabel +
                            ": essay sebaiknya memiliki rubrik/pembahasan untuk membantu koreksi"
                        );
                    }

                    if (!questionHasError)
                    {
                        result.SuccessMessages.Add(
                            questionLabel +
                            ": soal essay sudah valid"
                        );
                    }
                }
                else
                {
                    result.Errors.Add(
                        questionLabel +
                        ": tipe soal belum didukung. Saat ini hanya Multiple Choice dan Essay"
                    );
                }
            }

            var totalScore =
                questions.Sum(x =>
                    x.Score
                );

            if (totalScore <= 0)
            {
                result.Errors.Add("Total score quiz harus lebih dari 0");
            }
            else
            {
                result.SuccessMessages.Add("Total score quiz sudah valid: " + totalScore);
            }

            if (
                quiz.PassingScore > 0 &&
                quiz.PassingScore > totalScore
            )
            {
                result.Errors.Add(
                    "Passing score tidak boleh lebih besar dari total score quiz"
                );
            }
            else if (quiz.PassingScore > 0)
            {
                result.SuccessMessages.Add(
                    "Passing score sesuai dengan total score quiz"
                );
            }

            result.TotalQuestions =
                questions.Count;

            result.TotalScore =
                totalScore;

            result.IsValid =
                result.Errors.Count == 0;

            if (result.IsValid)
            {
                result.SuccessMessages.Add("Quiz siap dipublish");
            }

            return result;
        }






    }
}