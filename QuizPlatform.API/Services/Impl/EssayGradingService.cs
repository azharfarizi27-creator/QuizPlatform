using QuizPlatform.API.Models.DTO;
using QuizPlatform.API.Models.Dtos;
using QuizPlatform.API.Services.Base;
using QuizPlatform.API.Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QuizPlatform.API.Services.Impl
{
    public class EssayGradingService : BaseServices, IEssayGradingService
    {
        public List<EssayPendingDto> GetPendingEssayAnswers()
        {
            var answers =
                context.UserAnswers
                    .Where(x =>
                        x.QuestionOptionId == null &&
                        x.EssayAnswer != null &&
                        x.EssayAnswer != "" &&
                        x.IsCorrect == null
                    )
                    .OrderByDescending(x =>
                        x.AnsweredAt
                    )
                    .ToList();
            
            var questions =
                context.Questions.ToList();

            var attempts =
                context.QuizAttempts.ToList();

            var users =
                context.Users.ToList();

            var quizzes =
                context.Quizzes.ToList();

            var result =
                answers.Select(answer =>
                {
                    var question =
                        questions.FirstOrDefault(x =>
                            x.Id == answer.QuestionId
                        );

                    var attempt =
                        attempts.FirstOrDefault(x =>
                            x.Id == answer.AttemptId
                        );

                    var user =
                        attempt != null
                            ? users.FirstOrDefault(x =>
                                x.Id == attempt.UserId
                            )
                            : null;

                    var quiz =
                        attempt != null
                            ? quizzes.FirstOrDefault(x =>
                                x.Id == attempt.QuizId
                            )
                            : null;

                    return new EssayPendingDto
                    {
                        AnswerId =
                            answer.Id,

                        AttemptId =
                            answer.AttemptId,

                        UserId =
                            attempt != null
                                ? attempt.UserId
                                : 0,

                        StudentName =
                            user != null
                                ? user.FullName
                                : "Unknown Student",

                        QuizId =
                            attempt != null
                                ? attempt.QuizId
                                : 0,

                        QuizTitle =
                            quiz != null
                                ? quiz.Title
                                : "Unknown Quiz",

                        QuestionId =
                            answer.QuestionId,

                        QuestionText =
                            question != null
                                ? question.QuestionText
                                : "-",

                        EssayAnswer =
                            answer.EssayAnswer,

                        MaxScore =
                            question != null
                                ? question.Score
                                : 0,

                        EarnedScore =
                            answer.EarnedScore,

                        IsCorrect =
                            answer.IsCorrect
                    };
                })
                .ToList();

            return result;
        }

        public void GradeEssay(GradeEssayDto request)
        {
            if (request == null)
                throw new Exception("Data nilai kosong");

            var answer =
                context.UserAnswers
                    .FirstOrDefault(x =>
                        x.Id == request.AnswerId
                    );

            if (answer == null)
                throw new Exception("Jawaban essay tidak ditemukan");

            var question =
                context.Questions
                    .FirstOrDefault(x =>
                        x.Id == answer.QuestionId
                    );

            if (question == null)
                throw new Exception("Soal tidak ditemukan");

            if (question.QuestionTypeId != 3)
                throw new Exception("Jawaban ini bukan soal essay");

            if (request.EarnedScore < 0)
                throw new Exception("Nilai tidak boleh kurang dari 0");

            if (request.EarnedScore > question.Score)
                throw new Exception("Nilai tidak boleh melebihi score maksimal soal");

            var attempt =
                context.QuizAttempts
                    .FirstOrDefault(x =>
                        x.Id == answer.AttemptId
                    );

            if (attempt == null)
                throw new Exception("Attempt tidak ditemukan");

            answer.EarnedScore =
                request.EarnedScore;

            answer.IsCorrect =
                request.IsCorrect;

            answer.AnsweredAt =
                DateTime.Now;

            context.SaveChanges();

            RecalculateAttemptScore(attempt.Id);

            var refreshedAttempt =
                context.QuizAttempts
                    .FirstOrDefault(x =>
                        x.Id == attempt.Id
                    );

            if (refreshedAttempt == null)
                throw new Exception("Attempt tidak ditemukan setelah update nilai");

            var stillHasPendingEssay =
                HasPendingEssay(refreshedAttempt.Id);

            if (stillHasPendingEssay)
            {
                RemoveLeaderboard(
                    refreshedAttempt.UserId,
                    refreshedAttempt.QuizId
                );

                UpdateRanking(refreshedAttempt.QuizId);

                return;
            }

            CreateOrUpdateLeaderboard(refreshedAttempt);

            UpdateRanking(refreshedAttempt.QuizId);

            CreateEssayGradedNotification(refreshedAttempt);
        }

        private void RecalculateAttemptScore(int attemptId)
        {
            var attempt =
                context.QuizAttempts
                    .FirstOrDefault(x =>
                        x.Id == attemptId
                    );

            if (attempt == null)
                return;

            var totalScore =
                context.UserAnswers
                    .Where(x =>
                        x.AttemptId == attemptId
                    )
                    .Sum(x =>
                        x.EarnedScore ?? 0
                    );

            attempt.TotalScore =
                totalScore;

            context.SaveChanges();
        }
        private void CreateEssayGradedNotification(
    Models.Entity.QuizAttempt attempt
)
        {
            var quiz =
                context.Quizzes
                    .FirstOrDefault(x =>
                        x.Id == attempt.QuizId
                    );

            var quizTitle =
                quiz != null
                    ? quiz.Title
                    : "Quiz";

            var notificationService =
                new NotificationService();

            notificationService.CreateOrUpdateNotification(
                attempt.UserId,
                "Essay sudah dikoreksi",
                "Jawaban essay kamu pada quiz " +
                    quizTitle +
                    " sudah selesai dikoreksi. Nilai terbaru kamu: " +
                    attempt.TotalScore,
                "/result/" + attempt.Id,
                "ESSAY_GRADED",
                attempt.Id
            );
        }

        private bool HasPendingEssay(int attemptId)
        {
            var pendingEssayExists =
                (
                    from userAnswer in context.UserAnswers
                    join essayQuestion in context.Questions
                        on userAnswer.QuestionId equals essayQuestion.Id
                    where
                        userAnswer.AttemptId == attemptId &&
                        essayQuestion.QuestionTypeId == 3 &&
                        userAnswer.IsCorrect == null
                    select userAnswer.Id
                )
                .Any();

            return pendingEssayExists;
        }

        private void CreateOrUpdateLeaderboard(Models.Entity.QuizAttempt attempt)
        {
            if (attempt.EndTime == null)
                return;

            var durationInSeconds =
                (int)(attempt.EndTime.Value - attempt.StartTime)
                    .TotalSeconds;

            var existing =
                context.Leaderboards
                    .FirstOrDefault(x =>
                        x.UserId == attempt.UserId &&
                        x.QuizId == attempt.QuizId
                    );

            if (existing != null)
            {
                existing.Score =
                    attempt.TotalScore;

                existing.DurationInSeconds =
                    durationInSeconds;

                existing.CreatedAt =
                    DateTime.Now;
            }
            else
            {
                var leaderboard =
                    new Models.Entity.Leaderboard
                    {
                        UserId =
                            attempt.UserId,

                        QuizId =
                            attempt.QuizId,

                        Score =
                            attempt.TotalScore,

                        DurationInSeconds =
                            durationInSeconds,

                        RankPosition =
                            0,

                        CreatedAt =
                            DateTime.Now
                    };

                context.Leaderboards.Add(leaderboard);
            }

            context.SaveChanges();
        }

        private void RemoveLeaderboard(int userId, int quizId)
        {
            var existing =
                context.Leaderboards
                    .FirstOrDefault(x =>
                        x.UserId == userId &&
                        x.QuizId == quizId
                    );

            if (existing != null)
            {
                context.Leaderboards.Remove(existing);
                context.SaveChanges();
            }
        }

        private void UpdateRanking(int quizId)
        {
            var list =
                context.Leaderboards
                    .Where(x =>
                        x.QuizId == quizId
                    )
                    .OrderByDescending(x =>
                        x.Score
                    )
                    .ThenBy(x =>
                        x.DurationInSeconds
                    )
                    .ToList();

            int rank =
                1;

            foreach (var item in list)
            {
                item.RankPosition =
                    rank;

                rank++;
            }

            context.SaveChanges();
        }
    } 
}