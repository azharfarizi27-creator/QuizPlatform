using QuizPlatform.API.Models.DTO;
using QuizPlatform.API.Models.Dtos;
using QuizPlatform.API.Services.Base;
using QuizPlatform.API.Services.Interface;
using System.Collections.Generic;
using System.Linq;

namespace QuizPlatform.API.Services.Impl
{
    public class TeacherReportService : BaseServices, ITeacherReportService
    {
        public TeacherReportService()
            : base()
        {

        }

        public List<TeacherQuizResultDto> GetTeacherAnalytics()
        {
            var attempts =
                context.QuizAttempts
                    .Where(x =>
                        x.EndTime != null
                    )
                    .ToList();

            var result =
                attempts.Select(attempt =>
                {
                    var user =
                        context.Users
                            .FirstOrDefault(u =>
                                u.Id == attempt.UserId
                            );

                    var quiz =
                        context.Quizzes
                            .FirstOrDefault(q =>
                                q.Id == attempt.QuizId
                            );

                    var answers =
                        context.UserAnswers
                            .Where(a =>
                                a.AttemptId == attempt.Id
                            )
                            .ToList();

                    var essayQuestionIds =
                        context.Questions
                            .Where(q =>
                                q.QuizId == attempt.QuizId &&
                                q.QuestionTypeId == 3
                            )
                            .Select(q =>
                                q.Id
                            )
                            .ToList();

                    var pendingEssayCount =
                        answers.Count(a =>
                            essayQuestionIds.Contains(a.QuestionId) &&
                            a.IsCorrect == null
                        );

                    var hasPendingEssay =
                        pendingEssayCount > 0;

                    var passingScore =
                        quiz != null
                            ? quiz.PassingScore
                            : 0;

                    bool? isPassed =
                        null;

                    string resultStatus;

                    if (hasPendingEssay)
                    {
                        resultStatus =
                            "Menunggu Koreksi";

                        isPassed =
                            null;
                    }
                    else if (attempt.TotalScore >= passingScore)
                    {
                        resultStatus =
                            "Lulus";

                        isPassed =
                            true;
                    }
                    else
                    {
                        resultStatus =
                            "Belum Lulus";

                        isPassed =
                            false;
                    }

                    return new TeacherQuizResultDto
                    {
                        AttemptId =
                            attempt.Id,

                        StudentName =
                            user != null &&
                            !string.IsNullOrWhiteSpace(user.FullName)
                                ? user.FullName
                                : "Student #" + attempt.UserId,

                        QuizTitle =
                            quiz != null
                                ? quiz.Title
                                : "-",

                        Score =
                            attempt.TotalScore,

                        PassingScore =
                            passingScore,

                        TotalCorrect =
                            answers.Count(a =>
                                a.IsCorrect == true
                            ),

                        TotalWrong =
                            answers.Count(a =>
                                a.IsCorrect == false
                            ),

                        DurationInSeconds =
                            attempt.EndTime.HasValue
                                ? (int)(attempt.EndTime.Value - attempt.StartTime)
                                    .TotalSeconds
                                : 0,

                        StartTime =
                            attempt.StartTime,

                        EndTime =
                            attempt.EndTime.Value,

                        IsPassed =
                            isPassed,

                        ResultStatus =
                            resultStatus,

                        HasPendingEssay =
                            hasPendingEssay,

                        PendingEssayCount =
                            pendingEssayCount
                    };
                })
                .OrderByDescending(x =>
                    x.EndTime
                )
                .ToList();

            return result;
        }

        public TeacherStatsSummaryDto GetTeacherStatsSummary()
        {
            var attempts =
                context.QuizAttempts
                    .Where(x =>
                        x.EndTime != null
                    )
                    .ToList();

            int passedCount =
                0;

            int failedCount =
                0;

            double totalScore =
                0;

            int completedCount =
                0;

            foreach (var attempt in attempts)
            {
                var quiz =
                    context.Quizzes
                        .FirstOrDefault(q =>
                            q.Id == attempt.QuizId
                        );

                var answers =
                    context.UserAnswers
                        .Where(a =>
                            a.AttemptId == attempt.Id
                        )
                        .ToList();

                var essayQuestionIds =
                    context.Questions
                        .Where(q =>
                            q.QuizId == attempt.QuizId &&
                            q.QuestionTypeId == 3
                        )
                        .Select(q =>
                            q.Id
                        )
                        .ToList();

                var hasPendingEssay =
                    answers.Any(a =>
                        essayQuestionIds.Contains(a.QuestionId) &&
                        a.IsCorrect == null
                    );

                if (hasPendingEssay)
                {
                    continue;
                }

                var passingScore =
                    quiz != null
                        ? quiz.PassingScore
                        : 0;

                if (attempt.TotalScore >= passingScore)
                    passedCount++;
                else
                    failedCount++;

                totalScore +=
                    attempt.TotalScore;

                completedCount++;
            }

            var totalFinal =
                passedCount + failedCount;

            return new TeacherStatsSummaryDto
            {
                AverageQuizScore =
                    completedCount > 0
                        ? totalScore / completedCount
                        : 0,

                PassedCount =
                    passedCount,

                FailedCount =
                    failedCount,

                PassRate =
                    totalFinal > 0
                        ? ((double)passedCount / totalFinal) * 100
                        : 0
            };
        }

        public List<TopStudentDto> GetTopStudents()
        {
            var attempts =
                context.QuizAttempts
                    .Where(x =>
                        x.EndTime != null
                    )
                    .ToList();

            var finalAttempts =
                attempts.Where(attempt =>
                {
                    var answers =
                        context.UserAnswers
                            .Where(a =>
                                a.AttemptId == attempt.Id
                            )
                            .ToList();

                    var essayQuestionIds =
                        context.Questions
                            .Where(q =>
                                q.QuizId == attempt.QuizId &&
                                q.QuestionTypeId == 3
                            )
                            .Select(q =>
                                q.Id
                            )
                            .ToList();

                    var hasPendingEssay =
                        answers.Any(a =>
                            essayQuestionIds.Contains(a.QuestionId) &&
                            a.IsCorrect == null
                        );

                    return !hasPendingEssay;
                })
                .ToList();

            var data =
                finalAttempts
                    .GroupBy(x =>
                        x.UserId
                    )
                    .Select(g =>
                    {
                        var user =
                            context.Users
                                .FirstOrDefault(u =>
                                    u.Id == g.Key
                                );

                        return new TopStudentDto
                        {
                            UserId =
                                g.Key,

                            StudentName =
                                user != null &&
                                !string.IsNullOrWhiteSpace(user.FullName)
                                    ? user.FullName
                                    : "Student #" + g.Key,

                            TotalAttempts =
                                g.Count(),

                            HighestScore =
                                g.Max(x =>
                                    x.TotalScore
                                ),

                            AverageScore =
                                g.Average(x =>
                                    x.TotalScore
                                )
                        };
                    })
                    .OrderByDescending(x =>
                        x.HighestScore
                    )
                    .ThenByDescending(x =>
                        x.AverageScore
                    )
                    .Take(10)
                    .ToList();

            return data;
        }

        public List<QuestionAnalyticsDto> GetQuestionAnalytics()
        {
            var questions =
                context.Questions
                    .ToList();

            var result =
                questions.Select(q =>
                {
                    var answers =
                        context.UserAnswers
                            .Where(a =>
                                a.QuestionId == q.Id
                            )
                            .ToList();

                    var totalAnswered =
                        answers.Count;

                    var totalCorrect =
                        answers.Count(a =>
                            a.IsCorrect == true
                        );

                    var totalWrong =
                        answers.Count(a =>
                            a.IsCorrect == false
                        );

                    return new QuestionAnalyticsDto
                    {
                        QuestionId =
                            q.Id,

                        QuestionText =
                            q.QuestionText,

                        QuizTitle =
                            context.Quizzes
                                .Where(x =>
                                    x.Id == q.QuizId
                                )
                                .Select(x =>
                                    x.Title
                                )
                                .FirstOrDefault(),

                        TotalAnswered =
                            totalAnswered,

                        TotalCorrect =
                            totalCorrect,

                        TotalWrong =
                            totalWrong,

                        CorrectPercentage =
                            totalAnswered > 0
                                ? ((double)totalCorrect / totalAnswered) * 100
                                : 0
                    };
                })
                .OrderBy(x =>
                    x.CorrectPercentage
                )
                .ToList();

            return result;
        }
    }
}