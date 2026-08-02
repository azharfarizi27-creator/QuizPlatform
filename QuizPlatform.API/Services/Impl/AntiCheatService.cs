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
    public class AntiCheatService : BaseServices, IAntiCheatService
    {
        public void CreateLog(CreateSuspiciousActivityDto request)
        {
            if (request == null)
                throw new Exception("Data log kosong");

            if (request.AttemptId <= 0)
                throw new Exception("AttemptId tidak valid");

            if (string.IsNullOrWhiteSpace(request.Reason))
                throw new Exception("Reason wajib diisi");

            var attempt =
                context.QuizAttempts
                    .FirstOrDefault(x =>
                        x.Id == request.AttemptId
                    );

            if (attempt == null)
                throw new Exception("Attempt tidak ditemukan");

            var log =
                new QuizSuspiciousActivity
                {
                    AttemptId =
                        request.AttemptId,

                    UserId =
                        attempt.UserId,

                    QuizId =
                        attempt.QuizId,

                    Reason =
                        request.Reason,

                    WarningCount =
                        request.WarningCount,

                    CreatedAt =
                        DateTime.Now
                };

            context.QuizSuspiciousActivities.Add(log);
            context.SaveChanges();
        }

        public List<AntiCheatLogDto> GetLogs()
        {
            var logs =
                context.QuizSuspiciousActivities
                    .OrderByDescending(x => x.CreatedAt)
                    .Take(200)
                    .ToList();

            var users =
                context.Users
                    .ToList();

            var quizzes =
                context.Quizzes
                    .ToList();

            var result =
                logs.Select(log =>
                {
                    var user =
                        users.FirstOrDefault(x =>
                            x.Id == log.UserId
                        );

                    var quiz =
                        quizzes.FirstOrDefault(x =>
                            x.Id == log.QuizId
                        );

                    return new AntiCheatLogDto
                    {
                        Id =
                            log.Id,

                        AttemptId =
                            log.AttemptId,

                        UserId =
                            log.UserId,

                        FullName =
                            user != null
                                ? user.FullName
                                : "Unknown User",

                        Username =
                            user != null
                                ? user.Username
                                : "-",

                        QuizId =
                            log.QuizId,

                        QuizTitle =
                            quiz != null
                                ? quiz.Title
                                : "Unknown Quiz",

                        Reason =
                            log.Reason,

                        WarningCount =
                            log.WarningCount,

                        CreatedAt =
                            log.CreatedAt
                    };
                })
                .ToList();

            return result;
        }
    }
}