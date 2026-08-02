using QuizPlatform.API.Models.DTO;
using QuizPlatform.API.Models.Dtos;
using QuizPlatform.API.Models.Entity;
using QuizPlatform.API.Services.Base;
using QuizPlatform.API.Services.Interface;
using System;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;

namespace QuizPlatform.API.Services.Impl
{
    public class QuizAttemptService : BaseServices, IQuizAttemptService
    {

        private void FixNullTotalScoreByAttemptId(int attemptId)
        {
            context.Database.ExecuteSqlCommand(
                @"
        UPDATE qa
        SET qa.TotalScore = ISNULL(score.TotalScore, 0)
        FROM dbo.QuizAttempts qa
        OUTER APPLY (
            SELECT 
                SUM(ISNULL(ua.EarnedScore, 0)) AS TotalScore
            FROM dbo.UserAnswers ua
            WHERE ua.AttemptId = qa.Id
        ) score
        WHERE qa.Id = @AttemptId
          AND qa.TotalScore IS NULL
        ",
                new SqlParameter("@AttemptId", attemptId)
            );
        }

        private void FixNullTotalScoresForUser(int userId)
        {
            context.Database.ExecuteSqlCommand(
                @"
        UPDATE qa
        SET qa.TotalScore = ISNULL(score.TotalScore, 0)
        FROM dbo.QuizAttempts qa
        OUTER APPLY (
            SELECT 
                SUM(ISNULL(ua.EarnedScore, 0)) AS TotalScore
            FROM dbo.UserAnswers ua
            WHERE ua.AttemptId = qa.Id
        ) score
        WHERE qa.UserId = @UserId
          AND qa.TotalScore IS NULL
        ",
                new SqlParameter("@UserId", userId)
            );
        }

        private void FixNullTotalScoresForQuiz(int quizId)
        {
            context.Database.ExecuteSqlCommand(
                @"
        UPDATE qa
        SET qa.TotalScore = ISNULL(score.TotalScore, 0)
        FROM dbo.QuizAttempts qa
        OUTER APPLY (
            SELECT 
                SUM(ISNULL(ua.EarnedScore, 0)) AS TotalScore
            FROM dbo.UserAnswers ua
            WHERE ua.AttemptId = qa.Id
        ) score
        WHERE qa.QuizId = @QuizId
          AND qa.TotalScore IS NULL
        ",
                new SqlParameter("@QuizId", quizId)
            );
        }

        private void FixNullLeaderboardsForQuiz(int quizId)
        {
            context.Database.ExecuteSqlCommand(
                @"
        UPDATE dbo.Leaderboards
        SET 
            Score = ISNULL(Score, 0),
            DurationInSeconds = ISNULL(DurationInSeconds, 0),
            RankPosition = ISNULL(RankPosition, 0)
        WHERE QuizId = @QuizId
        ",
                new SqlParameter("@QuizId", quizId)
            );
        }

        private void FixNullTotalScoresForUserQuiz(int userId, int quizId)
        {
            context.Database.ExecuteSqlCommand(
                @"
        UPDATE qa
        SET qa.TotalScore = ISNULL(score.TotalScore, 0)
        FROM dbo.QuizAttempts qa
        OUTER APPLY (
            SELECT 
                SUM(ISNULL(ua.EarnedScore, 0)) AS TotalScore
            FROM dbo.UserAnswers ua
            WHERE ua.AttemptId = qa.Id
        ) score
        WHERE qa.UserId = @UserId
          AND qa.QuizId = @QuizId
          AND qa.TotalScore IS NULL
        ",
                new SqlParameter("@UserId", userId),
                new SqlParameter("@QuizId", quizId)
            );
        }
        public QuizAttemptService()
            : base()
        {

        }

        public QuizAttempt StartQuiz(
            QuizAttempt attempt
        )
        {
            var quiz =
                context.Quizzes
                    .FirstOrDefault(x =>
                        x.Id == attempt.QuizId
                    );

            if (quiz == null)
                throw new Exception("Quiz tidak ditemukan");

            if (quiz.Status == "Draft")
                throw new Exception("Quiz belum dipublish");

            if (quiz.Status == "Closed")
                throw new Exception("Quiz sudah ditutup");

            if (
                quiz.Status != "Published" &&
                quiz.Status != "Active"
            )
            {
                throw new Exception("Quiz belum tersedia");
            }

            var now =
                DateTime.Now;

            if (
                quiz.StartDate.HasValue &&
                now < quiz.StartDate.Value
            )
            {
                throw new Exception(
                    "Quiz belum bisa dikerjakan. Mulai pada: " +
                    quiz.StartDate.Value.ToString("dd MMM yyyy HH:mm")
                );
            }

            if (
                quiz.EndDate.HasValue &&
                now > quiz.EndDate.Value
            )
            {
                throw new Exception(
                    "Waktu pengerjaan quiz sudah berakhir"
                );
            }

            FixNullTotalScoresForUserQuiz(
                attempt.UserId,
                attempt.QuizId
            );

            var runningAttempt =
                context.QuizAttempts
                    .FirstOrDefault(x =>
                        x.UserId == attempt.UserId &&
                        x.QuizId == attempt.QuizId &&
                        x.Status == true
                    );
            if (runningAttempt != null)
                return runningAttempt;


            var finishedAttemptCount =
                context.QuizAttempts
                    .Count(x =>
                        x.UserId == attempt.UserId &&
                        x.QuizId == attempt.QuizId &&
                        x.Status == false
                    );

            if (
                quiz.MaxAttempt > 0 &&
                finishedAttemptCount >= quiz.MaxAttempt
            )
            {
                throw new Exception(
                    "Kamu sudah mencapai batas maksimal pengerjaan quiz"
                );
            }

            attempt.StartTime =
                DateTime.Now;

            attempt.EndTime =
                null;

            attempt.TotalScore =
                0;

            attempt.Status =
                true;

            context.QuizAttempts.Add(attempt);
            context.SaveChanges();

            var randomizedQuestions =
                context.Questions
                    .Where(x =>
                        x.QuizId == attempt.QuizId
                    )
                    .OrderBy(x =>
                        Guid.NewGuid()
                    )
                    .ToList();

            int questionOrder =
                1;

            foreach (var question in randomizedQuestions)
            {
                context.AttemptQuestions.Add(
                    new AttemptQuestion
                    {
                        AttemptId = attempt.Id,
                        QuestionId = question.Id,
                        OrderNumber = questionOrder
                    }
                );

                var randomizedOptions =
                    context.QuestionOptions
                        .Where(x =>
                            x.QuestionId == question.Id
                        )
                        .OrderBy(x =>
                            Guid.NewGuid()
                        )
                        .ToList();

                int optionOrder =
                    1;

                foreach (var option in randomizedOptions)
                {
                    context.AttemptQuestionOptions.Add(
                        new AttemptQuestionOption
                        {
                            AttemptId = attempt.Id,
                            QuestionId = question.Id,
                            QuestionOptionId = option.Id,
                            OrderNumber = optionOrder
                        }
                    );

                    optionOrder++;
                }

                questionOrder++;
            }

            context.SaveChanges();

            return attempt;
        }
        public void SubmitAnswer(UserAnswer answer)
        {
            if (answer == null)
                throw new Exception("Jawaban kosong");

            if (answer.AttemptId <= 0)
                throw new Exception("AttemptId tidak valid");

            var attempt =
                context.QuizAttempts
                    .FirstOrDefault(x =>
                        x.Id == answer.AttemptId
                    );

            if (attempt == null)
                throw new Exception("Attempt tidak ditemukan");

            if (attempt.Status == false)
                throw new Exception("Quiz sudah selesai");

            Question question = null;

            if (answer.QuestionId > 0)
            {
                question =
                    context.Questions
                        .FirstOrDefault(x =>
                            x.Id == answer.QuestionId
                        );
            }

            if (
                question == null &&
                answer.QuestionOptionId.HasValue
            )
            {
                var selectedOption =
                    context.QuestionOptions
                        .FirstOrDefault(x =>
                            x.Id == answer.QuestionOptionId.Value
                        );

                if (selectedOption != null)
                {
                    question =
                        context.Questions
                            .FirstOrDefault(x =>
                                x.Id == selectedOption.QuestionId
                            );

                    answer.QuestionId =
                        selectedOption.QuestionId;
                }
            }

            if (question == null)
                throw new Exception("Soal tidak ditemukan");

            var attemptQuestionExists =
                context.AttemptQuestions
                    .Any(x =>
                        x.AttemptId == answer.AttemptId &&
                        x.QuestionId == question.Id
                    );

            if (!attemptQuestionExists)
                throw new Exception("Soal tidak termasuk dalam attempt ini");

            var existingAnswer =
                context.UserAnswers
                    .FirstOrDefault(x =>
                        x.AttemptId == answer.AttemptId &&
                        x.QuestionId == question.Id
                    );

            // 1 = Multiple Choice
            // 3 = Essay
            var isMultipleChoice =
                question.QuestionTypeId == 1;

            var isEssay =
                question.QuestionTypeId == 3;

            if (!isMultipleChoice && !isEssay)
            {
                throw new Exception(
                    "Tipe soal belum didukung untuk submit jawaban"
                );
            }

            if (isEssay)
            {
                if (string.IsNullOrWhiteSpace(answer.EssayAnswer))
                    throw new Exception("Jawaban essay wajib diisi");

                if (existingAnswer == null)
                {
                    existingAnswer =
                        new UserAnswer
                        {
                            AttemptId =
                                answer.AttemptId,

                            QuestionId =
                                question.Id,

                            QuestionOptionId =
                                null,

                            EssayAnswer =
                                answer.EssayAnswer.Trim(),

                            IsCorrect =
                                null,

                            EarnedScore =
                                null,
                            AnsweredAt =
                                DateTime.Now
                        };

                    context.UserAnswers.Add(existingAnswer);
                }
                else
                {
                    existingAnswer.QuestionOptionId =
                        null;

                    existingAnswer.EssayAnswer =
                        answer.EssayAnswer.Trim();

                    existingAnswer.IsCorrect =
                        null;

                    existingAnswer.EarnedScore =
                        null;

                    existingAnswer.AnsweredAt =
                        DateTime.Now;
                }

                context.SaveChanges();
                return;
            }

            if (!answer.QuestionOptionId.HasValue)
                throw new Exception("Pilihan jawaban wajib dipilih");

            var option =
                context.QuestionOptions
                    .FirstOrDefault(x =>
                        x.Id == answer.QuestionOptionId.Value &&
                        x.QuestionId == question.Id
                    );

            if (option == null)
                throw new Exception("Pilihan jawaban tidak ditemukan");

            var isCorrect =
                option.IsCorrect;

            var earnedScore =
                isCorrect
                    ? question.Score
                    : 0;

            if (existingAnswer == null)
            {
                existingAnswer =
                    new UserAnswer
                    {
                        AttemptId =
                            answer.AttemptId,

                        QuestionId =
                            question.Id,

                        QuestionOptionId =
                            option.Id,

                        EssayAnswer =
                            null,

                        IsCorrect =
                            isCorrect,

                        EarnedScore =
                            earnedScore,

                        AnsweredAt =
                            DateTime.Now
                    };

                context.UserAnswers.Add(existingAnswer);
            }
            else
            {
                existingAnswer.QuestionOptionId =
                    option.Id;

                existingAnswer.EssayAnswer =
                    null;

                existingAnswer.IsCorrect =
                    isCorrect;

                existingAnswer.EarnedScore =
                    earnedScore;

                existingAnswer.AnsweredAt =
                    DateTime.Now;
            }

            context.SaveChanges();
        }
        public void EndQuiz(
            int attemptId
        )
        {
            FixNullTotalScoreByAttemptId(attemptId);

            var attempt =
                context.QuizAttempts
                    .FirstOrDefault(x =>
                        x.Id == attemptId
                    );

            if (attempt == null)
                throw new Exception(
                    "Attempt tidak ditemukan. AttemptId: " + attemptId
                );

            if (attempt.Status == false)
                return;

            var earnedScores =
                context.UserAnswers
                    .Where(x =>
                        x.AttemptId == attemptId
                    )
                    .Select(x =>
                        x.EarnedScore ?? 0
                    )
                    .ToList();

            var totalScore =
                earnedScores.Count > 0
                    ? earnedScores.Sum()
                    : 0;

            attempt.TotalScore =
                totalScore;

            attempt.EndTime =
                DateTime.Now;

            attempt.Status =
                false;

            context.SaveChanges();

            FixNullTotalScoreByAttemptId(attemptId);
            FixNullLeaderboardsForQuiz(attempt.QuizId);

            CreateLeaderboard(attemptId);
        }

        public void CreateLeaderboard(
      int attemptId
  )
        {
            FixNullTotalScoreByAttemptId(attemptId);
            var attempt =
                context.QuizAttempts
                    .FirstOrDefault(x =>
                        x.Id == attemptId
                    );

            if (attempt == null)
                throw new Exception("Attempt tidak ditemukan");

            FixNullLeaderboardsForQuiz(attempt.QuizId);
            
            if (attempt.EndTime == null)
                throw new Exception("Quiz belum selesai");

            var existing =
                context.Leaderboards
                    .FirstOrDefault(x =>
                        x.UserId == attempt.UserId &&
                        x.QuizId == attempt.QuizId
                    );

            if (HasPendingEssay(attempt.Id))
            {
                if (existing != null)
                {
                    context.Leaderboards.Remove(existing);
                    context.SaveChanges();
                }

                UpdateRanking(attempt.QuizId);

                return;
            }

            var durationInSeconds =
                (int)(attempt.EndTime.Value - attempt.StartTime)
                    .TotalSeconds;

            if (existing != null)
            {
                existing.Score =
                    attempt.TotalScore;

                existing.DurationInSeconds =
                    durationInSeconds;

                existing.CreatedAt =
                    DateTime.Now;

                context.SaveChanges();

                UpdateRanking(attempt.QuizId);

                return;
            }

            var leaderboard =
                new Leaderboard
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
            context.SaveChanges();

            UpdateRanking(attempt.QuizId);
        }
        public void UpdateRanking(
            int quizId
        )
        {
            FixNullTotalScoresForQuiz(quizId);
            FixNullLeaderboardsForQuiz(quizId);

            var allLeaderboards =
                context.Leaderboards
                    .Where(x =>
                        x.QuizId == quizId
                    )
                    .ToList();

            foreach (var item in allLeaderboards)
            {
                item.RankPosition =
                    0;
            }

            var validLeaderboards =
                allLeaderboards
                    .Where(x =>
                        IsLeaderboardEntryFinal(
                            x.UserId,
                            x.QuizId
                        )
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

            foreach (var item in validLeaderboards)
            {
                item.RankPosition =
                    rank;

                rank++;
            }

            context.SaveChanges();
        }

        public List<LeaderboardDto> GetLeaderboard(int quizId)
        {
            FixNullTotalScoresForQuiz(quizId);
            UpdateRanking(quizId);

            var leaderboards =
                context.Leaderboards
                    .Where(x =>
                        x.QuizId == quizId &&
                        x.RankPosition > 0
                    )
                    .OrderBy(x =>
                        x.RankPosition
                    )
                    .ToList();

            var userIds =
                leaderboards
                    .Select(x =>
                        x.UserId
                    )
                    .Distinct()
                    .ToList();

            var users =
                context.Users
                    .Where(x =>
                        userIds.Contains(x.Id)
                    )
                    .ToList();

            var result =
                leaderboards.Select(item =>
                {
                    var user =
                        users.FirstOrDefault(x =>
                            x.Id == item.UserId
                        );

                    var studentName =
                        user != null &&
                        !string.IsNullOrWhiteSpace(user.FullName)
                            ? user.FullName
                            : "Student #" + item.UserId;

                    return new LeaderboardDto
                    {
                        Id =
                            item.Id,

                        UserId =
                            item.UserId,

                        QuizId =
                            item.QuizId,

                        StudentName =
                            studentName,

                        Score =
                            item.Score,

                        DurationInSeconds =
                            item.DurationInSeconds,

                        RankPosition =
                            item.RankPosition,

                        CreatedAt =
                            item.CreatedAt
                    };
                })
                .ToList();

            return result;
        }

        private bool HasPendingEssay(
    int attemptId
)
        {
            var pendingEssayExists =
                (
                    from answer in context.UserAnswers
                    join question in context.Questions
                        on answer.QuestionId equals question.Id
                    where
                        answer.AttemptId == attemptId &&
                        question.QuestionTypeId == 3 &&
                        answer.IsCorrect == null
                    select answer.Id
                )
                .Any();

            return pendingEssayExists;
        }

        private bool IsLeaderboardEntryFinal(
            int userId,
            int quizId
        )
        {
            FixNullTotalScoresForUserQuiz(userId, quizId);
            var latestAttempt =
                context.QuizAttempts
                    .Where(x =>
                        x.UserId == userId &&
                        x.QuizId == quizId &&
                        x.EndTime != null
                    )
                    .OrderByDescending(x =>
                        x.EndTime
                    )
                    .FirstOrDefault();

            if (latestAttempt == null)
                return false;

            return !HasPendingEssay(latestAttempt.Id);
        }

        public QuizResultDto GetQuizResult(
            int attemptId
        )
        {
            FixNullTotalScoreByAttemptId(attemptId);
            var attempt =
                context.QuizAttempts
                    .FirstOrDefault(x =>
                        x.Id == attemptId
                    );

            if (attempt == null)
                throw new Exception("Attempt tidak ditemukan");

            var quiz =
                context.Quizzes
                    .FirstOrDefault(x =>
                        x.Id == attempt.QuizId
                    );

            var answers =
                context.UserAnswers
                    .Where(x =>
                        x.AttemptId == attemptId
                    )
                    .ToList();

            var attemptQuestions =
                context.AttemptQuestions
                    .Where(x =>
                        x.AttemptId == attemptId
                    )
                    .OrderBy(x =>
                        x.OrderNumber
                    )
                    .ToList();

            var questions =
                context.Questions
                    .Where(x =>
                        x.QuizId == attempt.QuizId
                    )
                    .ToList();

            var options =
                context.QuestionOptions
                    .ToList();

            var details =
                attemptQuestions
                    .Select(aq =>
                    {
                        var question =
                            questions.FirstOrDefault(q =>
                                q.Id == aq.QuestionId
                            );

                        if (question == null)
                            return null;

                        var answer =
                            answers.FirstOrDefault(x =>
                                x.QuestionId == question.Id
                            );

                        var isEssay =
                            question.QuestionTypeId == 3;

                        string userAnswer =
                            "";

                        string correctAnswer =
                            "";

                        if (isEssay)
                        {
                            userAnswer =
                                answer != null
                                    ? answer.EssayAnswer ?? ""
                                    : "";

                            correctAnswer =
                                "-";
                        }
                        else
                        {
                            userAnswer =
                                answer != null && answer.QuestionOptionId.HasValue
                                    ? options
                                        .Where(o =>
                                            o.Id == answer.QuestionOptionId.Value
                                        )
                                        .Select(o =>
                                            o.OptionText
                                        )
                                        .FirstOrDefault() ?? ""
                                    : "";

                            correctAnswer =
                                options
                                    .Where(o =>
                                        o.QuestionId == question.Id &&
                                        o.IsCorrect
                                    )
                                    .Select(o =>
                                        o.OptionText
                                    )
                                    .FirstOrDefault() ?? "";
                        }

                        return new QuizResultDetailDto
                        {
                            QuestionId =
                                question.Id,

                            QuestionTypeId =
                                question.QuestionTypeId,

                            QuestionType =
                                isEssay
                                    ? "Essay"
                                    : "Multiple Choice",

                            QuestionText =
                                question.QuestionText,

                            UserAnswer =
                                userAnswer,

                            SelectedAnswer =
                                userAnswer,

                            CorrectAnswer =
                                correctAnswer,

                            Explanation =
                                question.Explanation,

                            IsCorrect =
                                answer != null
                                    ? answer.IsCorrect
                                    : null,

                            Score =
                                answer != null
                                    ? answer.EarnedScore ?? 0
                                    : 0,

                            MaxScore =
                                question.Score
                        };
                    })
                    .Where(x =>
                        x != null
                    )
                    .ToList();

            var totalCorrect =
                answers.Count(x =>
                    x.IsCorrect == true
                );

            var totalWrong =
                answers.Count(x =>
                    x.IsCorrect == false
                );

            var result =
                new QuizResultDto
                {
                    AttemptId =
                        attempt.Id,

                    UserId =
                        attempt.UserId,

                    QuizId =
                        attempt.QuizId,

                    TotalScore =
                        attempt.TotalScore,

                    TotalCorrect =
                        totalCorrect,

                    TotalWrong =
                        totalWrong,

                    StartTime =
                        attempt.StartTime,

                    EndTime =
                        attempt.EndTime ?? DateTime.Now,

                    DurationInSeconds =
                        attempt.EndTime.HasValue
                            ? (int)(attempt.EndTime.Value - attempt.StartTime)
                                .TotalSeconds
                            : 0,

                    Details =
                        details
                };

            return result;
        }



        public List<QuizHistoryDto> GetStudentQuizHistory(int userId)
        {
            FixNullTotalScoresForUser(userId);
            var attempts =
                context.QuizAttempts
                    .Where(x =>
                        x.UserId == userId
                    )
                    .OrderByDescending(x =>
                        x.StartTime
                    )
                    .ToList();

            var result =
                attempts.Select(attempt =>
                {
                    var quiz =
                        context.Quizzes
                            .FirstOrDefault(x =>
                                x.Id == attempt.QuizId
                            );

                    var answers =
                        context.UserAnswers
                            .Where(x =>
                                x.AttemptId == attempt.Id
                            )
                            .ToList();

                    var essayQuestionIds =
                        context.Questions
                            .Where(x =>
                                x.QuizId == attempt.QuizId &&
                                x.QuestionTypeId == 3
                            )
                            .Select(x =>
                                x.Id
                            )
                            .ToList();

                    var pendingEssayCount =
                        answers.Count(x =>
                            essayQuestionIds.Contains(x.QuestionId) &&
                            x.IsCorrect == null
                        );

                    var hasPendingEssay =
                        pendingEssayCount > 0;

                    var isCompleted =
                        attempt.Status == false ||
                        attempt.EndTime.HasValue;

                    bool? isPassed =
                        null;

                    string resultStatus =
                        isCompleted
                            ? "Selesai"
                            : "Berjalan";

                    if (isCompleted)
                    {
                        if (hasPendingEssay)
                        {
                            resultStatus =
                                "Menunggu Koreksi";

                            isPassed =
                                null;
                        }
                        else
                        {
                            var passingScore =
                                quiz != null
                                    ? quiz.PassingScore
                                    : 0;

                            isPassed =
                                attempt.TotalScore >= passingScore;

                            resultStatus =
                                isPassed == true
                                    ? "Lulus"
                                    : "Belum Lulus";
                        }
                    }

                    var durationInSeconds =
                        attempt.EndTime.HasValue
                            ? (int)(attempt.EndTime.Value - attempt.StartTime)
                                .TotalSeconds
                            : 0;

                    return new QuizHistoryDto
                    {
                        AttemptId =
                            attempt.Id,

                        UserId =
                            attempt.UserId,

                        QuizId =
                            attempt.QuizId,

                        QuizTitle =
                            quiz != null
                                ? quiz.Title
                                : "-",

                        StartTime =
                            attempt.StartTime,

                        EndTime =
                            attempt.EndTime,

                        TotalScore =
                            attempt.TotalScore,

                        Status =
                            attempt.Status,

                        PassingScore =
                            quiz != null
                                ? quiz.PassingScore
                                : 0,

                        DurationInSeconds =
                            durationInSeconds,

                        IsPassed =
                            isPassed,

                        HasPendingEssay =
                            hasPendingEssay,

                        PendingEssayCount =
                            pendingEssayCount,

                        ResultStatus =
                            resultStatus
                    };
                })
                .ToList();

            return result;
        }
    }
}