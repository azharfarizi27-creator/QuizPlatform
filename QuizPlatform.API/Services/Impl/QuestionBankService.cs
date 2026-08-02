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
    public class QuestionBankService : BaseServices, IQuestionBankService
    {
        private readonly IActivityLogService activityLogService =
            new ActivityLogService();

        public QuestionBankService()
            : base()
        {

        }

        public List<QuestionBank> GetQuestionBanks()
        {
            return context.QuestionBanks
                .OrderByDescending(x => x.CreatedAt)
                .ToList();
        }

        public QuestionBank GetQuestionBankById(
            int id
        )
        {
            return context.QuestionBanks
                .FirstOrDefault(x => x.Id == id);
        }

        public QuestionBank CreateQuestionBank(
            QuestionBank bank
        )
        {
            if (bank == null)
                throw new Exception("Data bank soal kosong");

            if (string.IsNullOrWhiteSpace(bank.Name))
                throw new Exception("Nama bank soal wajib diisi");

            bank.CreatedAt =
                DateTime.Now;

            context.QuestionBanks.Add(bank);
            context.SaveChanges();

            activityLogService.CreateActivityLog(
                bank.CreatedBy,
                "CREATE_QUESTION_BANK",
                "Teacher/Admin membuat bank soal: " + bank.Name
            );

            return bank;
        }

        public void UpdateQuestionBank(
            QuestionBank bank
        )
        {
            if (bank == null)
                throw new Exception("Data bank soal kosong");

            var existing =
                context.QuestionBanks
                    .FirstOrDefault(x => x.Id == bank.Id);

            if (existing == null)
                throw new Exception("Bank soal tidak ditemukan");

            existing.Name =
                bank.Name;

            existing.Description =
                bank.Description;

            context.SaveChanges();

            activityLogService.CreateActivityLog(
                existing.CreatedBy,
                "UPDATE_QUESTION_BANK",
                "Teacher/Admin mengupdate bank soal: " + existing.Name
            );
        }

        public void DeleteQuestionBank(
            int id
        )
        {
            var bank =
                context.QuestionBanks
                    .FirstOrDefault(x => x.Id == id);

            if (bank == null)
                throw new Exception("Bank soal tidak ditemukan");

            var questions =
                context.Questions
                    .Where(x => x.QuestionBankId == id)
                    .ToList();

            foreach (var question in questions)
            {
                question.QuestionBankId = null;
            }

            context.QuestionBanks.Remove(bank);
            context.SaveChanges();

            activityLogService.CreateActivityLog(
                bank.CreatedBy,
                "DELETE_QUESTION_BANK",
                "Teacher/Admin menghapus bank soal: " + bank.Name
            );
        }

        public List<Question> GetQuestionsByBank(
            int bankId
        )
        {
            return context.Questions
                .Where(x => x.QuestionBankId == bankId)
                .OrderBy(x => x.OrderNumber)
                .ToList();
        }

        public Question CopyQuestionToQuiz(
            int questionId,
            int quizId
        )
        {
            var originalQuestion =
                context.Questions
                    .FirstOrDefault(x => x.Id == questionId);

            if (originalQuestion == null)
                throw new Exception("Soal tidak ditemukan");

            var quiz =
                context.Quizzes
                    .FirstOrDefault(x => x.Id == quizId);

            if (quiz == null)
                throw new Exception("Quiz tidak ditemukan");

            // Cek apakah soal sudah pernah ditambahkan
            var alreadyExists =
                context.Questions.Any(x =>
                    x.QuizId == quizId &&
                    x.QuestionBankId == originalQuestion.QuestionBankId &&
                    x.QuestionText == originalQuestion.QuestionText
                );

            if (alreadyExists)
                throw new Exception(
                    "Soal tersebut sudah ditambahkan ke quiz"
                );

            var lastOrder =
                context.Questions
                    .Where(x => x.QuizId == quizId)
                    .Select(x => x.OrderNumber)
                    .DefaultIfEmpty(0)
                    .Max();

            var newQuestion =
                new Question
                {
                    QuizId = quizId,
                    QuestionTypeId = originalQuestion.QuestionTypeId,
                    QuestionText = originalQuestion.QuestionText,
                    QuestionImage = originalQuestion.QuestionImage,
                    Explanation = originalQuestion.Explanation,
                    Score = originalQuestion.Score,
                    OrderNumber = lastOrder + 1,
                    QuestionBankId = originalQuestion.QuestionBankId,
                    CreatedAt = DateTime.Now
                };

            context.Questions.Add(newQuestion);

            // Save supaya Id question baru terbentuk
            context.SaveChanges();

            var options =
                context.QuestionOptions
                    .Where(x => x.QuestionId == questionId)
                    .ToList();

            foreach (var option in options)
            {
                context.QuestionOptions.Add(
                    new QuestionOption
                    {
                        QuestionId = newQuestion.Id,
                        OptionText = option.OptionText,
                        IsCorrect = option.IsCorrect,
                        OrderNumber = option.OrderNumber,
                        CreatedAt = DateTime.Now
                    }
                );
            }

            context.SaveChanges();

            activityLogService.CreateActivityLog(
                quiz.CreatedBy,
                "COPY_QUESTION_TO_QUIZ",
                "Teacher/Admin menambahkan soal dari bank soal ke quiz: "
                + quiz.Title
            );

            return newQuestion;
        }

        public int CopyRandomQuestionsFromBankToQuiz(
       CopyRandomQuestionDto request
   )
        {
            if (request == null)
                throw new Exception("Data request kosong");

            if (request.TotalQuestion <= 0)
                throw new Exception(
                    "Jumlah soal harus lebih dari 0"
                );

            var quiz =
                context.Quizzes
                    .FirstOrDefault(x => x.Id == request.QuizId);

            if (quiz == null)
                throw new Exception(
                    "Quiz tidak ditemukan"
                );

            var bank =
                context.QuestionBanks
                    .FirstOrDefault(x =>
                        x.Id == request.QuestionBankId
                    );

            if (bank == null)
                throw new Exception(
                    "Bank soal tidak ditemukan"
                );

            var bankQuestions =
                context.Questions
                    .Where(x =>
                        x.QuestionBankId ==
                        request.QuestionBankId
                    )
                    .ToList()
                    .OrderBy(x => Guid.NewGuid())
                    .ToList();

            if (bankQuestions.Count == 0)
                throw new Exception(
                    "Bank soal belum memiliki soal"
                );

            // Ambil sesuai jumlah yang diminta
            var selectedQuestions =
                bankQuestions
                    .Take(request.TotalQuestion)
                    .ToList();

            var lastOrder =
                context.Questions
                    .Where(x =>
                        x.QuizId == request.QuizId
                    )
                    .Select(x => x.OrderNumber)
                    .DefaultIfEmpty(0)
                    .Max();

            int copiedCount = 0;

            foreach (var originalQuestion in selectedQuestions)
            {
                // Jangan copy soal yang sudah ada
                var alreadyExists =
                    context.Questions.Any(x =>
                        x.QuizId == request.QuizId &&
                        x.QuestionBankId ==
                            originalQuestion.QuestionBankId &&
                        x.QuestionText ==
                            originalQuestion.QuestionText
                    );

                if (alreadyExists)
                    continue;

                lastOrder++;

                var newQuestion =
                    new Question
                    {
                        QuizId = request.QuizId,
                        QuestionTypeId =
                            originalQuestion.QuestionTypeId,
                        QuestionText =
                            originalQuestion.QuestionText,
                        QuestionImage =
                            originalQuestion.QuestionImage,
                        Explanation =
                            originalQuestion.Explanation,
                        Score =
                            originalQuestion.Score,
                        OrderNumber =
                            lastOrder,
                        QuestionBankId =
                            originalQuestion.QuestionBankId,
                        CreatedAt =
                            DateTime.Now
                    };

                context.Questions.Add(newQuestion);

                context.SaveChanges();

                var options =
                    context.QuestionOptions
                        .Where(x =>
                            x.QuestionId ==
                            originalQuestion.Id
                        )
                        .ToList();

                foreach (var option in options)
                {
                    context.QuestionOptions.Add(
                        new QuestionOption
                        {
                            QuestionId =
                                newQuestion.Id,
                            OptionText =
                                option.OptionText,
                            IsCorrect =
                                option.IsCorrect,
                            OrderNumber =
                                option.OrderNumber,
                            CreatedAt =
                                DateTime.Now
                        }
                    );
                }

                context.SaveChanges();

                copiedCount++;
            }

            activityLogService.CreateActivityLog(
                quiz.CreatedBy,
                "COPY_RANDOM_QUESTION",
                "Teacher/Admin mengambil "
                + copiedCount
                + " soal random dari bank soal: "
                + bank.Name
                + " ke quiz: "
                + quiz.Title
            );

            return copiedCount;
        }
    }
}