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
    public class ActivityLogService : BaseServices, IActivityLogService
    {
       
        public ActivityLogService()
            : base()
        {

        }

        public void CreateActivityLog(
            int? userId,
            string action,
            string description
        )
        {
            var log =
                new ActivityLog
                {
                    UserId = userId,
                    Action = action,
                    Description = description,
                    CreatedAt = DateTime.Now
                };

            context.ActivityLogs.Add(log);
            context.SaveChanges();
        }

        public List<ActivityLogDto> GetActivityLogs()
        {
            var logs =
                context.ActivityLogs
                    .OrderByDescending(x => x.CreatedAt)
                    .Take(100)
                    .ToList();

            var users =
                context.Users
                    .ToList();

            var result =
                logs.Select(log =>
                {
                    var user =
                        users.FirstOrDefault(x =>
                            x.Id == log.UserId
                        );

                    return new ActivityLogDto
                    {
                        Id = log.Id,
                        UserId = log.UserId,

                        FullName =
                            user != null
                                ? user.FullName
                                : "System",

                        Username =
                            user != null
                                ? user.Username
                                : "-",

                        Action = log.Action,
                        Description = log.Description,
                        CreatedAt = log.CreatedAt
                    };
                })
                .ToList();

            return result;
        }
    }
}