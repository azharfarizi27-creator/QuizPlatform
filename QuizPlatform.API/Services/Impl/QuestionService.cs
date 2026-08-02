using QuizPlatform.API.Models.Dtos;
using QuizPlatform.API.Models.Entity;
using QuizPlatform.API.Services.Base;
using QuizPlatform.API.Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QuizPlatform.API.Services.Impl
{
    public class QuestionService : BaseServices, IQuestionService
    {
        public QuestionService()
            : base()
        {

        }

        public List<Question> GetAllQuestions()
        {
            return context.Questions
                .OrderBy(x => x.QuizId)
                .ThenBy(x => x.OrderNumber)
                .ToList();
        }

        public void CreateQuestion(
            Question question
        )
        {
            if (question == null)
                throw new Exception("Data soal kosong");

            if (question.QuizId <= 0)
                throw new Exception("QuizId wajib diisi");

            if (string.IsNullOrWhiteSpace(question.QuestionText))
                throw new Exception("Teks soal wajib diisi");

            if (question.Score <= 0)
                throw new Exception("Score soal harus lebih dari 0");

            question.CreatedAt =
                DateTime.Now;

            context.Questions.Add(question);
            context.SaveChanges();
        }

        public void UpdateQuestion(
            Question question
        )
        {
            if (question == null)
                throw new Exception("Data soal kosong");

            var existing =
                context.Questions
                    .FirstOrDefault(x => x.Id == question.Id);

            if (existing == null)
                throw new Exception("Soal tidak ditemukan");

            existing.QuestionText =
                question.QuestionText;

            existing.Score =
                question.Score;

            existing.OrderNumber =
                question.OrderNumber;

            existing.Explanation =
                question.Explanation;

            existing.QuestionImage =
                question.QuestionImage;

            context.SaveChanges();
        }

        public void DeleteQuestion(
            int questionId
        )
        {
            var question =
                context.Questions
                    .FirstOrDefault(x => x.Id == questionId);

            if (question == null)
                throw new Exception("Soal tidak ditemukan");

            var hasAnswer =
                context.UserAnswers
                    .Any(x => x.QuestionId == questionId);

            if (hasAnswer)
                throw new Exception(
                    "Soal tidak bisa dihapus karena sudah pernah dijawab student."
                );

            var attemptQuestions =
                context.AttemptQuestions
                    .Where(x => x.QuestionId == questionId)
                    .ToList();

            var attemptOptions =
                context.AttemptQuestionOptions
                    .Where(x => x.QuestionId == questionId)
                    .ToList();

            var options =
                context.QuestionOptions
                    .Where(x => x.QuestionId == questionId)
                    .ToList();

            context.AttemptQuestionOptions.RemoveRange(attemptOptions);
            context.AttemptQuestions.RemoveRange(attemptQuestions);
            context.QuestionOptions.RemoveRange(options);
            context.Questions.Remove(question);

            context.SaveChanges();
        }

        public List<QuestionOption> GetAllQuestionOptions()
        {
            return context.QuestionOptions
                .OrderBy(x => x.QuestionId)
                .ThenBy(x => x.OrderNumber)
                .ToList();
        }

        public void CreateQuestionOption(
            QuestionOption option
        )
        {
            if (option == null)
                throw new Exception("Data jawaban kosong");

            if (option.QuestionId <= 0)
                throw new Exception("QuestionId wajib diisi");

            if (string.IsNullOrWhiteSpace(option.OptionText))
                throw new Exception("Teks jawaban wajib diisi");

            var question =
                context.Questions
                    .FirstOrDefault(x => x.Id == option.QuestionId);

            if (question == null)
                throw new Exception("Soal tidak ditemukan");

            if (option.IsCorrect)
            {
                var existingCorrect =
                    context.QuestionOptions
                        .FirstOrDefault(x =>
                            x.QuestionId == option.QuestionId &&
                            x.IsCorrect == true
                        );

                if (existingCorrect != null)
                    throw new Exception(
                        "Sudah ada jawaban benar untuk soal ini"
                    );
            }

            option.CreatedAt =
                DateTime.Now;

            context.QuestionOptions.Add(option);
            context.SaveChanges();
        }

        public void UpdateQuestionOption(
            QuestionOption option
        )
        {
            if (option == null)
                throw new Exception("Data jawaban kosong");

            var existing =
                context.QuestionOptions
                    .FirstOrDefault(x => x.Id == option.Id);

            if (existing == null)
                throw new Exception("Jawaban tidak ditemukan");

            if (option.IsCorrect)
            {
                var otherCorrect =
                    context.QuestionOptions
                        .FirstOrDefault(x =>
                            x.QuestionId == existing.QuestionId &&
                            x.Id != option.Id &&
                            x.IsCorrect == true
                        );

                if (otherCorrect != null)
                    throw new Exception(
                        "Sudah ada jawaban benar untuk soal ini"
                    );
            }

            existing.OptionText =
                option.OptionText;

            existing.OrderNumber =
                option.OrderNumber;

            existing.IsCorrect =
                option.IsCorrect;

            context.SaveChanges();
        }

        public void DeleteQuestionOption(
            int optionId
        )
        {
            var option =
                context.QuestionOptions
                    .FirstOrDefault(x => x.Id == optionId);

            if (option == null)
                throw new Exception("Jawaban tidak ditemukan");

            var hasAnswer =
                context.UserAnswers
                    .Any(x => x.QuestionOptionId == optionId);

            if (hasAnswer)
                throw new Exception(
                    "Jawaban tidak bisa dihapus karena sudah pernah dipilih student."
                );

            var attemptOptions =
                context.AttemptQuestionOptions
                    .Where(x => x.QuestionOptionId == optionId)
                    .ToList();

            context.AttemptQuestionOptions.RemoveRange(attemptOptions);
            context.QuestionOptions.Remove(option);

            context.SaveChanges();
        }

        public List<Question> GetQuestionsByAttempt(
            int attemptId
        )
        {
            var result =
                context.AttemptQuestions
                    .Where(x =>
                        x.AttemptId == attemptId
                    )
                    .OrderBy(x =>
                        x.OrderNumber
                    )
                    .Join(
                        context.Questions,
                        aq => aq.QuestionId,
                        q => q.Id,
                        (aq, q) => q
                    )
                    .ToList();

            return result;
        }

        public List<QuestionOption> GetOptionsByAttemptQuestion(
            int attemptId,
            int questionId
        )
        {
            var result =
                context.AttemptQuestionOptions
                    .Where(x =>
                        x.AttemptId == attemptId &&
                        x.QuestionId == questionId
                    )
                    .OrderBy(x =>
                        x.OrderNumber
                    )
                    .Join(
                        context.QuestionOptions,
                        aqo => aqo.QuestionOptionId,
                        qo => qo.Id,
                        (aqo, qo) => qo
                    )
                    .ToList();

            return result;
        }


        public int ImportQuestionsFromExcel(
    ImportQuestionExcelRequestDto request
)
        {
            if (request == null)
                throw new Exception("Data import kosong");

            if (request.QuizId <= 0)
                throw new Exception("QuizId tidak valid");

            if (
                request.Questions == null ||
                request.Questions.Count == 0
            )
            {
                throw new Exception("Data soal kosong");
            }

            var quiz =
                context.Quizzes
                    .FirstOrDefault(x =>
                        x.Id == request.QuizId
                    );

            if (quiz == null)
                throw new Exception("Quiz tidak ditemukan");

            var lastOrder =
                context.Questions
                    .Where(x =>
                        x.QuizId == request.QuizId
                    )
                    .Select(x =>
                        x.OrderNumber
                    )
                    .DefaultIfEmpty(0)
                    .Max();

            int importedCount =
                0;

            int rowNumber =
                1;

            foreach (var row in request.Questions)
            {
                rowNumber++;

                if (string.IsNullOrWhiteSpace(row.QuestionText))
                {
                    throw new Exception(
                        "Baris " + rowNumber + ": pertanyaan wajib diisi"
                    );
                }

                var correctAnswer =
                    (row.CorrectAnswer ?? "")
                        .Trim()
                        .ToUpper();

                var allowedAnswers =
                    new[] { "A", "B", "C", "D" };

                if (!allowedAnswers.Contains(correctAnswer))
                {
                    throw new Exception(
                        "Baris " + rowNumber + ": CorrectAnswer harus A, B, C, atau D"
                    );
                }

                var options =
                    new[]
                    {
                new { Key = "A", Text = row.OptionA },
                new { Key = "B", Text = row.OptionB },
                new { Key = "C", Text = row.OptionC },
                new { Key = "D", Text = row.OptionD }
                    };

                var filledOptions =
                    options
                        .Where(x =>
                            !string.IsNullOrWhiteSpace(x.Text)
                        )
                        .ToList();

                if (filledOptions.Count < 2)
                {
                    throw new Exception(
                        "Baris " + rowNumber + ": minimal harus ada 2 opsi jawaban"
                    );
                }

                var correctOption =
                    options.FirstOrDefault(x =>
                        x.Key == correctAnswer
                    );

                if (
                    correctOption == null ||
                    string.IsNullOrWhiteSpace(correctOption.Text)
                )
                {
                    throw new Exception(
                        "Baris " + rowNumber + ": opsi jawaban benar tidak boleh kosong"
                    );
                }

                lastOrder++;

                var question =
                    new Question
                    {
                        QuizId =
                            request.QuizId,

                        QuestionTypeId =
                            1,

                        QuestionText =
                            row.QuestionText.Trim(),

                        QuestionImage =
                            null,

                        Explanation =
                            row.Explanation,

                        Score =
                            row.Score > 0
                                ? row.Score
                                : 10,

                        OrderNumber =
                            lastOrder,

                        QuestionBankId =
                            row.QuestionBankId,

                        CreatedAt =
                            DateTime.Now
                    };

                context.Questions.Add(question);
                context.SaveChanges();

                int optionOrder =
                    1;

                foreach (var option in filledOptions)
                {
                    context.QuestionOptions.Add(
                        new QuestionOption
                        {
                            QuestionId =
                                question.Id,

                            OptionText =
                                option.Text.Trim(),

                            IsCorrect =
                                option.Key == correctAnswer,

                            OrderNumber =
                                optionOrder,

                            CreatedAt =
                                DateTime.Now
                        }
                    );

                    optionOrder++;
                }

                importedCount++;
            }

            context.SaveChanges();

            return importedCount;
        }

    }
}