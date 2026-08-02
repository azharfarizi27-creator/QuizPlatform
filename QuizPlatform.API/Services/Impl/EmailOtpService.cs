using QuizPlatform.API.Models.Entity;
using QuizPlatform.API.Services.Base;
using QuizPlatform.API.Services.Interface;
using System;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;

namespace QuizPlatform.API.Services.Impl
{
    public class EmailOtpService : BaseServices, IEmailOtpService
    {
        public EmailOtpService()
            : base()
        {

        }

        public void CreateAndSendOtp(
            int userId,
            string email,
            string purpose
        )
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new Exception("Email wajib diisi");

            if (string.IsNullOrWhiteSpace(purpose))
                throw new Exception("Purpose OTP wajib diisi");

            var oldOtps =
                context.EmailOtps
                    .Where(x =>
                        x.Email == email &&
                        x.Purpose == purpose &&
                        x.IsUsed == false
                    )
                    .ToList();

            foreach (var item in oldOtps)
            {
                item.IsUsed = true;
            }

            var code =
                GenerateOtpCode();

            var otp =
                new EmailOtp
                {
                    UserId = userId,
                    Email = email,
                    Code = code,
                    Purpose = purpose,
                    IsUsed = false,
                    ExpiredAt = DateTime.Now.AddMinutes(10),
                    CreatedAt = DateTime.Now
                };

            context.EmailOtps.Add(otp);
            context.SaveChanges();

            SendOtpEmail(
                email,
                code,
                purpose
            );
        }

        public bool ValidateAndUseOtp(
            string email,
            string code,
            string purpose
        )
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new Exception("Email wajib diisi");

            if (string.IsNullOrWhiteSpace(code))
                throw new Exception("Kode OTP wajib diisi");

            if (string.IsNullOrWhiteSpace(purpose))
                throw new Exception("Purpose OTP wajib diisi");

            var otp =
                context.EmailOtps
                    .Where(x =>
                        x.Email == email &&
                        x.Code == code &&
                        x.Purpose == purpose &&
                        x.IsUsed == false &&
                        x.ExpiredAt > DateTime.Now
                    )
                    .OrderByDescending(x => x.Id)
                    .FirstOrDefault();

            if (otp == null)
                return false;

            otp.IsUsed = true;

            context.SaveChanges();

            return true;
        }

        private string GenerateOtpCode()
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                var bytes = new byte[4];

                rng.GetBytes(bytes);

                var value =
                    BitConverter.ToUInt32(bytes, 0);

                var code =
                    (value % 900000) + 100000;

                return code.ToString();
            }
        }

        private void SendOtpEmail(
            string toEmail,
            string code,
            string purpose
        )
        {
            var host =
                ConfigurationManager.AppSettings["SmtpHost"];

            var port =
                int.Parse(
                    ConfigurationManager.AppSettings["SmtpPort"]
                );

            var smtpEmail =
                ConfigurationManager.AppSettings["SmtpEmail"];

            var smtpPassword =
                ConfigurationManager.AppSettings["SmtpPassword"];

            var smtpName =
                ConfigurationManager.AppSettings["SmtpName"];

            var subject =
                GetOtpSubject(purpose);

            var body = $@"
<div style='font-family: Arial; padding: 20px;'>
    <h2>{subject}</h2>

    <p>Gunakan kode berikut untuk melanjutkan proses:</p>

    <div style='font-size: 28px; font-weight: bold; letter-spacing: 6px; background: #f3f4f6; padding: 14px; border-radius: 10px; width: fit-content;'>
        {code}
    </div>

    <p style='margin-top: 20px; color: #6b7280;'>
        Kode ini berlaku selama 10 menit.
    </p>

    <p style='color: #6b7280;'>
        Jika kamu tidak meminta kode ini, abaikan email ini.
    </p>
</div>
";

            var mail =
                new MailMessage();

            mail.From =
                new MailAddress(
                    smtpEmail,
                    smtpName
                );

            mail.To.Add(toEmail);
            mail.Subject = subject;
            mail.Body = body;
            mail.IsBodyHtml = true;

            using (var smtp = new SmtpClient(host, port))
            {
                smtp.Credentials =
                    new NetworkCredential(
                        smtpEmail,
                        smtpPassword
                    );

                smtp.EnableSsl = true;

                smtp.Send(mail);
            }
        }

        private string GetOtpSubject(
            string purpose
        )
        {
            if (purpose == "REGISTER")
                return "Kode Verifikasi Register - Quiz Platform";

            if (purpose == "RESET_PASSWORD")
                return "Kode Reset Password - Quiz Platform";

            if (purpose == "CHANGE_PASSWORD")
                return "Kode Ganti Password - Quiz Platform";

            return "Kode OTP - Quiz Platform";
        }
    }
}