using QuizPlatform.API.Models.DTO;
using QuizPlatform.API.Models.Dtos;
using QuizPlatform.API.Models.Entity;
using QuizPlatform.API.Services.Context;
using QuizPlatform.API.Services.Interface;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace QuizPlatform.API.Services.Impl
{
    public class NotificationService : INotificationService
    {
        private readonly QuizDbContext context =
            new QuizDbContext();

        public NotificationSummaryDto GetSummary(int currentUserId)
        {
            var pendingFriendRequests =
                context.FriendRequests.Count(x =>
                    x.ReceiverId == currentUserId &&
                    x.Status == "Pending"
                );

            var unreadMessages =
                context.ChatMessages.Count(x =>
                    x.ReceiverId == currentUserId &&
                    x.IsRead == false
                );

            var quizNotification =
                GetQuizNotificationCounts(currentUserId);

            var unreadStudentNotifications =
                context.StudentNotifications.Count(x =>
                    x.UserId == currentUserId &&
                    x.IsRead == false
                );

            var total =
                pendingFriendRequests +
                unreadMessages +
                quizNotification.PendingQuizzes +
                quizNotification.DueSoonQuizzes +
                quizNotification.ExpiredQuizzes +
                unreadStudentNotifications;

            return new NotificationSummaryDto
            {
                PendingFriendRequests =
                    pendingFriendRequests,

                UnreadMessages =
                    unreadMessages,

                PendingQuizzes =
                    quizNotification.PendingQuizzes,

                DueSoonQuizzes =
                    quizNotification.DueSoonQuizzes,

                ExpiredQuizzes =
                    quizNotification.ExpiredQuizzes,

                Total =
                    total
            };
        }

        public List<NotificationItemDto> GetItems(int currentUserId)
        {
            var result =
                new List<NotificationItemDto>();

            AddStoredStudentNotificationItems(result, currentUserId);
            AddFriendRequestItems(result, currentUserId);
            AddUnreadChatItems(result, currentUserId);
            AddQuizItems(result, currentUserId);

            return result
                .OrderByDescending(x =>
                    x.CreatedAt
                )
                .ToList();
        }

        public void CreateOrUpdateNotification(
            int userId,
            string title,
            string message,
            string link,
            string notificationType,
            int? referenceId
        )
        {
            if (userId <= 0)
                throw new Exception("UserId tidak valid");

            if (string.IsNullOrWhiteSpace(title))
                throw new Exception("Title notifikasi wajib diisi");

            if (string.IsNullOrWhiteSpace(message))
                throw new Exception("Message notifikasi wajib diisi");

            if (string.IsNullOrWhiteSpace(notificationType))
                notificationType = "GENERAL";

            var existing =
                context.StudentNotifications
                    .FirstOrDefault(x =>
                        x.UserId == userId &&
                        x.NotificationType == notificationType &&
                        x.ReferenceId == referenceId
                    );

            if (existing != null)
            {
                existing.Title =
                    title;

                existing.Message =
                    message;

                existing.Link =
                    link;

                existing.IsRead =
                    false;

                existing.CreatedAt =
                    DateTime.Now;

                context.SaveChanges();
                return;
            }

            var notification =
                new StudentNotification
                {
                    UserId =
                        userId,

                    Title =
                        title,

                    Message =
                        message,

                    Link =
                        link,

                    NotificationType =
                        notificationType,

                    ReferenceId =
                        referenceId,

                    IsRead =
                        false,

                    CreatedAt =
                        DateTime.Now
                };

            context.StudentNotifications.Add(notification);
            context.SaveChanges();
        }

        public List<StudentNotificationDto> GetStudentNotifications(int userId)
        {
            return context.StudentNotifications
                .Where(x =>
                    x.UserId == userId
                )
                .OrderByDescending(x =>
                    x.CreatedAt
                )
                .Take(50)
                .Select(x =>
                    new StudentNotificationDto
                    {
                        Id =
                            x.Id,

                        UserId =
                            x.UserId,

                        Title =
                            x.Title,

                        Message =
                            x.Message,

                        Link =
                            x.Link,

                        NotificationType =
                            x.NotificationType,

                        ReferenceId =
                            x.ReferenceId,

                        IsRead =
                            x.IsRead,

                        CreatedAt =
                            x.CreatedAt
                    }
                )
                .ToList();
        }

        public int GetUnreadCount(int userId)
        {
            return context.StudentNotifications
                .Count(x =>
                    x.UserId == userId &&
                    x.IsRead == false
                );
        }

        public void MarkAsRead(int notificationId, int userId)
        {
            var notification =
                context.StudentNotifications
                    .FirstOrDefault(x =>
                        x.Id == notificationId &&
                        x.UserId == userId
                    );

            if (notification == null)
                throw new Exception("Notifikasi tidak ditemukan");

            notification.IsRead =
                true;

            context.SaveChanges();
        }

        public void MarkAllAsRead(int userId)
        {
            var notifications =
                context.StudentNotifications
                    .Where(x =>
                        x.UserId == userId &&
                        x.IsRead == false
                    )
                    .ToList();

            foreach (var notification in notifications)
            {
                notification.IsRead =
                    true;
            }

            context.SaveChanges();
        }

        private void AddStoredStudentNotificationItems(
            List<NotificationItemDto> result,
            int currentUserId
        )
        {
            var notifications =
                context.StudentNotifications
                    .Where(x =>
                        x.UserId == currentUserId
                    )
                    .OrderByDescending(x =>
                        x.CreatedAt
                    )
                    .Take(20)
                    .ToList();

            foreach (var notification in notifications)
            {
                result.Add(
                    new NotificationItemDto
                    {
                        Type =
                            notification.NotificationType,

                        Title =
                            notification.Title,

                        Message =
                            notification.Message,

                        TargetUrl =
                            notification.Link,

                        CreatedAt =
                            notification.CreatedAt
                    }
                );
            }
        }

        private void AddFriendRequestItems(
            List<NotificationItemDto> result,
            int currentUserId
        )
        {
            var requests = context.FriendRequests
                .Where(x =>
                    x.ReceiverId == currentUserId &&
                    x.Status == "Pending"
                )
                .OrderByDescending(x => x.CreatedAt)
                .ToList();

            foreach (var request in requests)
            {
                var requester = context.Users
                    .Include(x => x.Role)
                    .FirstOrDefault(x => x.Id == request.RequesterId);

                var name =
                    requester != null
                        ? requester.FullName
                        : "User";

                result.Add(new NotificationItemDto
                {
                    Type = "FriendRequest",
                    Title = "Request pertemanan baru",
                    Message = $"{name} mengirim request pertemanan.",
                    TargetUrl = "/friends",
                    CreatedAt = request.CreatedAt
                });
            }
        }

        private void AddUnreadChatItems(
            List<NotificationItemDto> result,
            int currentUserId
        )
        {
            var unreadGroups = context.ChatMessages
                .Where(x =>
                    x.ReceiverId == currentUserId &&
                    x.IsRead == false
                )
                .GroupBy(x => x.SenderId)
                .Select(x => new
                {
                    SenderId = x.Key,
                    Count = x.Count(),
                    LastMessageAt = x.Max(m => m.CreatedAt)
                })
                .ToList();

            foreach (var group in unreadGroups)
            {
                var sender = context.Users
                    .FirstOrDefault(x => x.Id == group.SenderId);

                var name =
                    sender != null
                        ? sender.FullName
                        : "User";

                result.Add(new NotificationItemDto
                {
                    Type = "UnreadChat",
                    Title = "Pesan baru",
                    Message = $"{group.Count} pesan baru dari {name}.",
                    TargetUrl = $"/chat/{group.SenderId}",
                    CreatedAt = group.LastMessageAt
                });
            }
        }

        private void AddQuizItems(
            List<NotificationItemDto> result,
            int currentUserId
        )
        {
            var user = context.Users
                .Include(x => x.Role)
                .FirstOrDefault(x => x.Id == currentUserId);

            var roleName =
                user != null && user.Role != null
                    ? user.Role.Name
                    : "";

            if (roleName != "Student")
                return;

            var attemptedQuizIds = context.QuizAttempts
                .Where(x => x.UserId == currentUserId)
                .Select(x => x.QuizId)
                .ToList();

            var now = DateTime.Now;
            var dueSoonLimit = now.AddDays(1);

            var quizzes = context.Quizzes
                .ToList()
                .Where(x =>
                    IsQuizActiveOrPublished(x) &&
                    !attemptedQuizIds.Contains(x.Id)
                )
                .ToList();

            foreach (var quiz in quizzes)
            {
                var title = GetQuizTitle(quiz);
                var endDate = GetQuizEndDate(quiz);
                var startDate = GetQuizStartDate(quiz);

                if (startDate.HasValue && startDate.Value > now)
                {
                    result.Add(new NotificationItemDto
                    {
                        Type = "QuizUpcoming",
                        Title = "Quiz akan dibuka",
                        Message = $"Quiz {title} akan dibuka pada {FormatDate(startDate.Value)}.",
                        TargetUrl = $"/quiz-confirm/{quiz.Id}",
                        CreatedAt = startDate.Value
                    });

                    continue;
                }

                if (endDate.HasValue && endDate.Value < now)
                {
                    result.Add(new NotificationItemDto
                    {
                        Type = "QuizExpired",
                        Title = "Quiz sudah lewat deadline",
                        Message = $"Quiz {title} sudah melewati deadline {FormatDate(endDate.Value)} dan belum dikerjakan.",
                        TargetUrl = $"/quiz-confirm/{quiz.Id}",
                        CreatedAt = endDate.Value
                    });

                    continue;
                }

                if (endDate.HasValue && endDate.Value <= dueSoonLimit)
                {
                    result.Add(new NotificationItemDto
                    {
                        Type = "QuizDueSoon",
                        Title = "Quiz deadline dekat",
                        Message = $"Quiz {title} harus dikerjakan sebelum {FormatDate(endDate.Value)}.",
                        TargetUrl = $"/quiz-confirm/{quiz.Id}",
                        CreatedAt = endDate.Value
                    });

                    continue;
                }

                result.Add(new NotificationItemDto
                {
                    Type = "PendingQuiz",
                    Title = "Quiz belum dikerjakan",
                    Message = $"Quiz {title} tersedia dan belum kamu kerjakan.",
                    TargetUrl = $"/quiz-confirm/{quiz.Id}",
                    CreatedAt = now
                });
            }
        }

        private NotificationSummaryDto GetQuizNotificationCounts(
            int currentUserId
        )
        {
            var result = new NotificationSummaryDto();

            var user = context.Users
                .Include(x => x.Role)
                .FirstOrDefault(x => x.Id == currentUserId);

            var roleName =
                user != null && user.Role != null
                    ? user.Role.Name
                    : "";

            if (roleName != "Student")
                return result;

            var attemptedQuizIds = context.QuizAttempts
                .Where(x => x.UserId == currentUserId)
                .Select(x => x.QuizId)
                .ToList();

            var now = DateTime.Now;
            var dueSoonLimit = now.AddDays(1);

            var quizzes = context.Quizzes
                .ToList()
                .Where(x =>
                    IsQuizActiveOrPublished(x) &&
                    !attemptedQuizIds.Contains(x.Id)
                )
                .ToList();

            foreach (var quiz in quizzes)
            {
                var startDate = GetQuizStartDate(quiz);
                var endDate = GetQuizEndDate(quiz);

                if (startDate.HasValue && startDate.Value > now)
                    continue;

                if (endDate.HasValue && endDate.Value < now)
                {
                    result.ExpiredQuizzes++;
                    continue;
                }

                if (endDate.HasValue && endDate.Value <= dueSoonLimit)
                {
                    result.DueSoonQuizzes++;
                    continue;
                }

                result.PendingQuizzes++;
            }

            return result;
        }

        private bool IsQuizActiveOrPublished(Quiz quiz)
        {
            var status = GetStringProperty(quiz, "Status");

            if (string.IsNullOrWhiteSpace(status))
                return true;

            return
                status == "Active" ||
                status == "Published";
        }

        private string GetQuizTitle(Quiz quiz)
        {
            return
                GetStringProperty(quiz, "Title") ??
                GetStringProperty(quiz, "QuizTitle") ??
                GetStringProperty(quiz, "Name") ??
                GetStringProperty(quiz, "QuizName") ??
                $"Quiz #{quiz.Id}";
        }

        private DateTime? GetQuizStartDate(Quiz quiz)
        {
            return
                GetDateProperty(quiz, "StartDate") ??
                GetDateProperty(quiz, "StartTime") ??
                GetDateProperty(quiz, "StartAt") ??
                GetDateProperty(quiz, "AvailableFrom");
        }

        private DateTime? GetQuizEndDate(Quiz quiz)
        {
            return
                GetDateProperty(quiz, "EndDate") ??
                GetDateProperty(quiz, "EndTime") ??
                GetDateProperty(quiz, "EndAt") ??
                GetDateProperty(quiz, "Deadline") ??
                GetDateProperty(quiz, "DueDate") ??
                GetDateProperty(quiz, "AvailableUntil");
        }

        private string GetStringProperty(object obj, string propertyName)
        {
            var property = obj.GetType().GetProperty(propertyName);

            if (property == null)
                return null;

            var value = property.GetValue(obj);

            if (value == null)
                return null;

            return value.ToString();
        }

        private DateTime? GetDateProperty(object obj, string propertyName)
        {
            var property = obj.GetType().GetProperty(propertyName);

            if (property == null)
                return null;

            var value = property.GetValue(obj);

            if (value == null)
                return null;

            if (value is DateTime)
                return (DateTime)value;

            if (value is DateTime?)
                return (DateTime?)value;

            DateTime parsed;

            if (DateTime.TryParse(value.ToString(), out parsed))
                return parsed;

            return null;
        }

        private string FormatDate(DateTime date)
        {
            return date.ToString("dd MMM yyyy HH:mm");
        }
    }
}