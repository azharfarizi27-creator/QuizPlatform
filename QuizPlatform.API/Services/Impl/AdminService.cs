using QuizPlatform.API.Models.DTO;
using QuizPlatform.API.Models.Dtos;
using QuizPlatform.API.Models.Entity;
using QuizPlatform.API.Models.Generator;
using QuizPlatform.API.Services.Base;
using QuizPlatform.API.Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QuizPlatform.API.Services.Impl
{
    public class AdminService : BaseServices, IAdminService
    {
        public AdminService()
            : base()
        {

        }

        public List<User> GetAllUser()
        {
            return context.Users
                .OrderByDescending(x => x.CreatedAt)
                .ToList();
        }

        public void CreateUser(User user)
        {
            if (user == null)
                throw new Exception("Data user tidak boleh kosong");

            if (string.IsNullOrWhiteSpace(user.FullName))
                throw new Exception("Nama lengkap wajib diisi");

            if (string.IsNullOrWhiteSpace(user.Username))
                throw new Exception("Username wajib diisi");

            if (string.IsNullOrWhiteSpace(user.Email))
                throw new Exception("Email wajib diisi");

            if (string.IsNullOrWhiteSpace(user.PasswordHash))
                throw new Exception("Password wajib diisi");

            if (user.RoleId <= 0)
                throw new Exception("Role wajib dipilih");

            user.FullName =
                user.FullName.Trim();

            user.Username =
                user.Username.Trim();

            user.Email =
                user.Email.Trim().ToLower();

            var roleExists =
                context.Roles.Any(x =>
                    x.Id == user.RoleId
                );

            if (!roleExists)
                throw new Exception("Role tidak ditemukan");

            var usernameExists =
                context.Users.Any(x =>
                    x.Username.ToLower() == user.Username.ToLower()
                );

            if (usernameExists)
                throw new Exception("Username sudah digunakan");

            var emailExists =
                context.Users.Any(x =>
                    x.Email.ToLower() == user.Email.ToLower()
                );

            if (emailExists)
                throw new Exception("Email sudah digunakan");

            user.PasswordHash =
                PasswordGenerator.GenerateHash(
                    user.PasswordHash
                );

            user.EmailVerified =
                true;

            user.IsActive =
                true;

            user.CreatedAt =
                DateTime.Now;

            context.Users.Add(user);
            context.SaveChanges();
        }
        public DashboardStatsDto GetDashboardStats()
        {
            return new DashboardStatsDto
            {
                TotalUsers =
                    context.Users.Count(),

                TotalStudents =
                    context.Users.Count(x =>
                        x.Role.Name == "Student"
                    ),

                TotalTeachers =
                    context.Users.Count(x =>
                        x.Role.Name == "Teacher"
                    ),

                TotalQuizzes =
                    context.Quizzes.Count(),

                TotalQuestions =
                    context.Questions.Count(),

                TotalAttempts =
                    context.QuizAttempts.Count()
            };
        }

        public List<AdminUserDto> GetAdminUsers()
        {
            var users =
                context.Users
                    .Include("Role")
                    .OrderByDescending(x => x.CreatedAt)
                    .Select(x => new AdminUserDto
                    {
                        Id = x.Id,
                        FullName = x.FullName,
                        Username = x.Username,
                        Email = x.Email,
                        RoleId = x.RoleId,
                        RoleName = x.Role.Name,
                        IsActive = x.IsActive,
                        CreatedAt = x.CreatedAt
                    })
                    .ToList();

            return users;
        }

        public void ChangeUserRole(
            ChangeUserRoleDto request
        )
        {
            var user =
                context.Users
                    .FirstOrDefault(x =>
                        x.Id == request.UserId
                    );

            if (user == null)
                throw new Exception("User tidak ditemukan");

            var role =
                context.Roles
                    .FirstOrDefault(x =>
                        x.Id == request.RoleId
                    );

            if (role == null)
                throw new Exception("Role tidak ditemukan");

            user.RoleId =
                request.RoleId;

            context.SaveChanges();
        }

        public void DeleteUser(
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

            user.IsActive =
                false;

            context.SaveChanges();
        }

        public void ActivateUser(
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

            user.IsActive =
                true;

            context.SaveChanges();
        }

        public List<Role> GetAllRoles()
        {
            return context.Roles
                .OrderBy(x => x.Id)
                .ToList();
        }
    }
}