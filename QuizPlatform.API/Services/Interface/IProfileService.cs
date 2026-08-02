using QuizPlatform.API.Models.DTO;
using QuizPlatform.API.Models.Dtos;
using System.Collections.Generic;

namespace QuizPlatform.API.Services.Interface
{
    public interface IProfileService
    {
        StudentProfileStatsDto GetStudentProfileStats(
            int userId
        );

        void UpdateProfile(
            int userId,
            UpdateProfileDto request
        );

        void ChangePassword(
            int userId,
            ChangePasswordDto request
        );

        void UpdateProfileImage(
            int userId,
            string profileImage
        );

        List<StudentNotificationDto> GetStudentNotifications(
            int userId
        );
    }
}