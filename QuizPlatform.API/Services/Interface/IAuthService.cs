using QuizPlatform.API.Models.DTO;
using QuizPlatform.API.Models.Dtos;
using QuizPlatform.API.Models.Entity;

namespace QuizPlatform.API.Services.Interface
{
    public interface IAuthService
    {
        object Login(
            string username,
            string password
        );

        void RegisterStudent(
            User user
        );

        void VerifyRegisterCode(
            VerifyRegisterCodeDto request
        );

        void ForgotPassword(
            ForgotPasswordDto request
        );

        void ResetPassword(
            ResetPasswordDto request
        );

        void RequestChangePasswordCode(
            int userId
        );

        void ChangePasswordWithCode(
            int userId,
            ChangePasswordWithCodeDto request
        );
    }
}