using QuizPlatform.API.Models.DTO;
using QuizPlatform.API.Models.Dtos;
using QuizPlatform.API.Models.Entity;
using QuizPlatform.API.Services.Impl;
using QuizPlatform.API.Services.Interface;
using System;
using System.Security.Claims;
using System.Web.Http;

namespace QuizPlatform.API.Controllers
{
    public class AuthApiController : ApiController
    {
        private readonly IAuthService service =
            new AuthService();

        [HttpPost]
        [Route("api/auth/login")]
        public IHttpActionResult Login(LoginDto dto)
        {
            try
            {
                if (dto == null)
                    return BadRequest("Data kosong");

                if (string.IsNullOrWhiteSpace(dto.Username))
                    return BadRequest("Username wajib diisi");

                if (string.IsNullOrWhiteSpace(dto.Password))
                    return BadRequest("Password wajib diisi");

                var result =
                    service.Login(
                        dto.Username,
                        dto.Password
                    );

                if (result == null)
                    return Unauthorized();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Route("api/Auth/RegisterStudent")]
        public IHttpActionResult RegisterStudent(
            [FromBody] User user
        )
        {
            try
            {
                if (user == null)
                    return BadRequest("Data kosong");

                service.RegisterStudent(user);

                return Ok(new
                {
                    message = "Register berhasil. Kode verifikasi sudah dikirim ke email.",
                    email = user.Email
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Route("api/Auth/VerifyRegisterCode")]
        public IHttpActionResult VerifyRegisterCode(
            VerifyRegisterCodeDto request
        )
        {
            try
            {
                service.VerifyRegisterCode(request);

                return Ok(
                    "Email berhasil diverifikasi. Silakan login."
                );
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Route("api/Auth/ForgotPassword")]
        public IHttpActionResult ForgotPassword(
            ForgotPasswordDto request
        )
        {
            try
            {
                service.ForgotPassword(request);

                return Ok(new
                {
                    message = "Kode reset password sudah dikirim ke email.",
                    email = request.Email
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Route("api/Auth/ResetPassword")]
        public IHttpActionResult ResetPassword(
            ResetPasswordDto request
        )
        {
            try
            {
                service.ResetPassword(request);

                return Ok(
                    "Password berhasil direset"
                );
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpPost]
        [Route("api/Auth/RequestChangePasswordCode")]
        public IHttpActionResult RequestChangePasswordCode()
        {
            try
            {
                int userId =
                    GetCurrentUserId();

                service.RequestChangePasswordCode(userId);

                return Ok(
                    "Kode ganti password sudah dikirim ke email."
                );
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpPost]
        [Route("api/Auth/ChangePasswordWithCode")]
        public IHttpActionResult ChangePasswordWithCode(
            ChangePasswordWithCodeDto request
        )
        {
            try
            {
                int userId =
                    GetCurrentUserId();

                service.ChangePasswordWithCode(
                    userId,
                    request
                );

                return Ok(
                    "Password berhasil diganti"
                );
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private int GetCurrentUserId()
        {
            var identity =
                User.Identity as ClaimsIdentity;

            if (identity == null)
                throw new Exception("Token tidak valid");

            var idClaim =
                identity.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                identity.FindFirst("UserId")?.Value ??
                identity.FindFirst("userId")?.Value ??
                identity.FindFirst("Id")?.Value ??
                identity.FindFirst("id")?.Value;

            if (!int.TryParse(idClaim, out int userId))
                throw new Exception("UserId tidak ditemukan di token");

            return userId;
        }
    }
}