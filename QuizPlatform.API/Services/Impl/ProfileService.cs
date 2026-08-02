using QuizPlatform.API.Models.DTO;
using QuizPlatform.API.Models.Dtos;
using QuizPlatform.API.Models.Generator;
using QuizPlatform.API.Services.Base;
using QuizPlatform.API.Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QuizPlatform.API.Services.Impl
{
    public class ProfileService : BaseServices, IProfileService
    {
        public ProfileService()
            : base()
        {

        }

        public StudentProfileStatsDto GetStudentProfileStats(
            int userId
        )
        {
            var user =
                context.Users
                    .FirstOrDefault(x =>
                        x.Id == userId
                    );

            if (user == null)
                throw new Exception("User tidak ditemukan");

            var attempts =
                context.QuizAttempts
                    .Where(x =>
                        x.UserId == userId &&
                        x.EndTime != null
                    )
                    .ToList();

            var passed =
                attempts.Count(x =>
                    x.TotalScore >= 75
                );

            return new StudentProfileStatsDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Username = user.Username,
                Email = user.Email,
                ProfileImage = user.ProfileImage,

                TotalQuiz = attempts.Count,

                PassedQuiz = passed,

                FailedQuiz =
                    attempts.Count - passed,

                HighestScore =
                    attempts.Any()
                        ? attempts.Max(x => x.TotalScore)
                        : 0,

                AverageScore =
                    attempts.Any()
                        ? attempts.Average(x => x.TotalScore)
                        : 0
            };
        }

        public void UpdateProfile(
            int userId,
            UpdateProfileDto request
        )
        {
            var user =
                context.Users
                    .FirstOrDefault(x =>
                        x.Id == userId
                    );

            if (user == null)
                throw new Exception("User tidak ditemukan");

            user.FullName =
                request.FullName;

            user.Username =
                request.Username;

            user.Email =
                request.Email;

            user.UpdatedAt =
                DateTime.Now;

            context.SaveChanges();
        }

        public void ChangePassword(
            int userId,
            ChangePasswordDto request
        )
        {
            var user =
                context.Users
                    .FirstOrDefault(x =>
                        x.Id == userId
                    );

            if (user == null)
                throw new Exception("User tidak ditemukan");

            string oldPasswordHash =
                PasswordGenerator.GenerateHash(
                    request.OldPassword
                );

            if (user.PasswordHash != oldPasswordHash)
                throw new Exception("Password lama salah");

            string newPasswordHash =
                PasswordGenerator.GenerateHash(
                    request.NewPassword
                );

            user.PasswordHash =
                newPasswordHash;

            user.UpdatedAt =
                DateTime.Now;

            context.SaveChanges();
        }

        public void UpdateProfileImage(
            int userId,
            string profileImage
        )
        {
            var user =
                context.Users
                    .FirstOrDefault(x =>
                        x.Id == userId
                    );

            if (user == null)
                throw new Exception("User tidak ditemukan");

            user.ProfileImage =
                profileImage;

            user.UpdatedAt =
                DateTime.Now;

            context.SaveChanges();
        }

        public List<StudentNotificationDto> GetStudentNotifications(
            int userId
        )
        {
            var attemptdQuizIds =
                context.QuizAttempts
                    .Where(x =>
                        x.UserId == userId
                    )
                    .Select(x =>
                        x.QuizId
                    )
                    .ToList();

            var quizzes =
                context.Quizzes
                    .Where(x =>
                        attemptdQuizIds.Contains(x.Id)
                    )
                    .OrderByDescending(x =>
                        x.CreatedAt
                    )
                    .Take(5)
                    .ToList();

            return quizzes.Select(x =>
                new StudentNotificationDto
                {
                    QuizId = x.Id,
                    QuizTitle = x.Title,
                    Message = "Quiz baru tersedia: " + x.Title,
                    CreatedAt = x.CreatedAt
                })
                .ToList();
        }
    }
}