using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace QuizPlatform.API.Services.Interface
{
    public interface IEmailOtpService
    {
        void CreateAndSendOtp(
            int userId,
            string email,
            string purpose
        );

        bool ValidateAndUseOtp(
            string email,
            string code,
            string purpose
        );
    }
}