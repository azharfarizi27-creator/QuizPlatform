using QuizPlatform.API.Models.DTO;
using QuizPlatform.API.Models.Dtos;
using System.Collections.Generic;

namespace QuizPlatform.API.Services.Interface
{
    public interface INotificationService
    {
        NotificationSummaryDto GetSummary(int currentUserId);

        List<NotificationItemDto> GetItems(int currentUserId);

        void CreateOrUpdateNotification(
            int userId,
            string title,
            string message,
            string link,
            string notificationType,
            int? referenceId
        );

        List<StudentNotificationDto> GetStudentNotifications(int userId);

        int GetUnreadCount(int userId);

        void MarkAsRead(int notificationId, int userId);

        void MarkAllAsRead(int userId);
    }
}