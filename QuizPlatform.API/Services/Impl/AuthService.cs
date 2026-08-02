using QuizPlatform.API.Helpers;
using QuizPlatform.API.Models.DTO;
using QuizPlatform.API.Models.Dtos;
using QuizPlatform.API.Models.Entity;
using QuizPlatform.API.Models.Generator;
using QuizPlatform.API.Services.Base;
using QuizPlatform.API.Services.Interface;
using System;
using System.Linq;

namespace QuizPlatform.API.Services.Impl
{
    public class AuthService : BaseServices, IAuthService
    {
        private readonly IEmailOtpService emailOtpService =
            new EmailOtpService();

        private readonly IActivityLogService activityLogService =
            new ActivityLogService();

        public AuthService()
            : base()
        {

        }

        public object Login(
            string username,
            string password
        )
        {
            string hashed =
                PasswordGenerator.GenerateHash(password);

            var user =
                context.Users
                    .Include("Role")
                    .FirstOrDefault(x =>
                        x.Username == username &&
                        x.PasswordHash == hashed &&
                        x.IsActive == true
                    );

            if (user == null)
                return null;

            return new
            {
                user.Id,
                user.FullName,
                user.Username,
                user.Email,
                user.RoleId,

                RoleName =
                    user.Role != null
                        ? user.Role.Name
                        : null,

                Token =
                    JwtHelper.GenerateToken(
                        user.Id,
                        user.Role.Name
                    )
            };
        }

        public void RegisterStudent(
            User user
        )
        {
            var existingUser =
                context.Users
                    .FirstOrDefault(x =>
                        x.Username == user.Username ||
                        x.Email == user.Email
                    );

            if (existingUser != null)
                throw new Exception("Username atau Email sudah digunakan");

            user.PasswordHash =
                PasswordGenerator.GenerateHash(
                    user.PasswordHash
                );

            user.RoleId = 3;
            user.IsActive = false;
            user.EmailVerified = false;
            user.CreatedAt = DateTime.Now;

            context.Users.Add(user);
            context.SaveChanges();

            emailOtpService.CreateAndSendOtp(
                user.Id,
                user.Email,
                "REGISTER"
            );

            activityLogService.CreateActivityLog(
                user.Id,
                "REGISTER_STUDENT",
                "Student register dan menunggu verifikasi email"
            );
        }

        public void VerifyRegisterCode(
            VerifyRegisterCodeDto request
        )
        {
            if (request == null)
                throw new Exception("Data kosong");

            if (string.IsNullOrWhiteSpace(request.Email))
                throw new Exception("Email wajib diisi");

            if (string.IsNullOrWhiteSpace(request.Code))
                throw new Exception("Kode wajib diisi");

            var user =
                context.Users
                    .FirstOrDefault(x =>
                        x.Email == request.Email
                    );

            if (user == null)
                throw new Exception("User tidak ditemukan");

            var isOtpValid =
                emailOtpService.ValidateAndUseOtp(
                    request.Email,
                    request.Code,
                    "REGISTER"
                );

            if (!isOtpValid)
                throw new Exception("Kode OTP salah atau sudah expired");

            user.EmailVerified = true;
            user.IsActive = true;
            user.UpdatedAt = DateTime.Now;

            context.SaveChanges();

            activityLogService.CreateActivityLog(
                user.Id,
                "VERIFY_REGISTER_EMAIL",
                "Student berhasil verifikasi email"
            );
        }

        public void ForgotPassword(
            ForgotPasswordDto request
        )
        {
            if (request == null)
                throw new Exception("Data kosong");

            if (string.IsNullOrWhiteSpace(request.Email))
                throw new Exception("Email wajib diisi");

            var user =
                context.Users
                    .FirstOrDefault(x =>
                        x.Email == request.Email
                    );

            if (user == null)
                throw new Exception("Email tidak ditemukan");

            emailOtpService.CreateAndSendOtp(
                user.Id,
                user.Email,
                "RESET_PASSWORD"
            );

            activityLogService.CreateActivityLog(
                user.Id,
                "FORGOT_PASSWORD",
                "User meminta kode reset password via email"
            );
        }

        public void ResetPassword(
            ResetPasswordDto request
        )
        {
            if (request == null)
                throw new Exception("Data kosong");

            if (string.IsNullOrWhiteSpace(request.Email))
                throw new Exception("Email wajib diisi");

            if (string.IsNullOrWhiteSpace(request.Code))
                throw new Exception("Kode wajib diisi");

            if (string.IsNullOrWhiteSpace(request.NewPassword))
                throw new Exception("Password baru wajib diisi");

            if (request.NewPassword.Length < 6)
                throw new Exception("Password baru minimal 6 karakter");

            if (request.NewPassword != request.ConfirmPassword)
                throw new Exception("Password konfirmasi tidak sama");

            var user =
                context.Users
                    .FirstOrDefault(x =>
                        x.Email == request.Email
                    );

            if (user == null)
                throw new Exception("User tidak ditemukan");

            var isOtpValid =
                emailOtpService.ValidateAndUseOtp(
                    request.Email,
                    request.Code,
                    "RESET_PASSWORD"
                );

            if (!isOtpValid)
                throw new Exception("Kode OTP salah atau sudah expired");

            user.PasswordHash =
                PasswordGenerator.GenerateHash(
                    request.NewPassword
                );

            user.ResetPasswordToken = null;
            user.ResetPasswordExpired = null;
            user.UpdatedAt = DateTime.Now;

            context.SaveChanges();

            activityLogService.CreateActivityLog(
                user.Id,
                "RESET_PASSWORD",
                "User berhasil reset password dengan OTP email"
            );
        }

        public void RequestChangePasswordCode(
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

            if (string.IsNullOrWhiteSpace(user.Email))
                throw new Exception("Email user belum tersedia");

            emailOtpService.CreateAndSendOtp(
                user.Id,
                user.Email,
                "CHANGE_PASSWORD"
            );

            activityLogService.CreateActivityLog(
                user.Id,
                "REQUEST_CHANGE_PASSWORD_CODE",
                "User meminta kode ganti password via email"
            );
        }

        public void ChangePasswordWithCode(
            int userId,
            ChangePasswordWithCodeDto request
        )
        {
            if (request == null)
                throw new Exception("Data kosong");

            if (string.IsNullOrWhiteSpace(request.Code))
                throw new Exception("Kode wajib diisi");

            if (string.IsNullOrWhiteSpace(request.NewPassword))
                throw new Exception("Password baru wajib diisi");

            if (request.NewPassword.Length < 6)
                throw new Exception("Password baru minimal 6 karakter");

            if (request.NewPassword != request.ConfirmPassword)
                throw new Exception("Konfirmasi password tidak sama");

            var user =
                context.Users
                    .FirstOrDefault(x =>
                        x.Id == userId
                    );

            if (user == null)
                throw new Exception("User tidak ditemukan");

            var isOtpValid =
                emailOtpService.ValidateAndUseOtp(
                    user.Email,
                    request.Code,
                    "CHANGE_PASSWORD"
                );

            if (!isOtpValid)
                throw new Exception("Kode OTP salah atau sudah expired");

            user.PasswordHash =
                PasswordGenerator.GenerateHash(
                    request.NewPassword
                );

            user.UpdatedAt = DateTime.Now;

            context.SaveChanges();

            activityLogService.CreateActivityLog(
                user.Id,
                "CHANGE_PASSWORD",
                "User berhasil mengganti password dengan OTP email"
            );
        }

  
    }
}