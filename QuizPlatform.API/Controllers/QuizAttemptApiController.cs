using Microsoft.Ajax.Utilities;
using QuizPlatform.API.Models.Entity;
using QuizPlatform.API.Services.Impl;
using QuizPlatform.API.Services.Interface;
using System;
using System.Security.Claims;
using System.Web.Http;

namespace QuizPlatform.API.Controllers
{
    public class QuizAttemptApiController : ApiController
    {
        private readonly IActivityLogService activityLogService =
            new ActivityLogService();

        private readonly IQuizAttemptService service =
            new QuizAttemptService();

        [Authorize]
        [HttpPost]
        [Route("api/QuizAttempt/Start")]
        public IHttpActionResult Start(
            [FromBody] QuizAttempt attempt
        )
        {
            var identity =
                User.Identity as ClaimsIdentity;

            if (identity == null)
                return Unauthorized();

            var roleClaim =
                identity.FindFirst(ClaimTypes.Role);

            if (roleClaim == null)
                return BadRequest("Role tidak ditemukan di token");

            if (roleClaim.Value != "Student")
                return Unauthorized();

            var userId =
                GetLoginUserId();

            if (userId == null)
                return BadRequest("UserId tidak ditemukan di token");

            if (attempt == null)
                return BadRequest("Request tidak boleh kosong");

            if (attempt.QuizId <= 0)
                return BadRequest("QuizId tidak valid");

            attempt.UserId =
                userId.Value;

            attempt.TotalScore =
                0;

            attempt.Status =
                true;

            try
            {
                var result =
                    service.StartQuiz(attempt);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(
                    ex.Message
                );
            }
        }

        [Authorize]
        [HttpPost]
        [Route("api/QuizAttempt/End")]
        public IHttpActionResult End(
            [FromBody] QuizAttempt request
        )
        {
            var identity =
                User.Identity as ClaimsIdentity;

            if (identity == null)
                return Unauthorized();

            var role =
                identity.FindFirst(
                    ClaimTypes.Role
                )?.Value;

            if (role != "Student")
                return Unauthorized();

            if (request == null)
                return BadRequest("Request tidak boleh kosong");

            if (request.Id <= 0)
                return BadRequest("AttemptId tidak valid");

            try
            {
                service.EndQuiz(
                    request.Id
                );

                var userId =
                    GetLoginUserId();

                activityLogService.CreateActivityLog(
                    userId,
                    "FINISH_QUIZ",
                    "Student menyelesaikan quiz attempt ID: " + request.Id
                );

                return Ok(
                    new
                    {
                        Message = "Quiz selesai",
                        AttemptId = request.Id
                    }
                );
            }
            catch (Exception ex)
            {
                var message =
                    ex.Message ?? "";

                if (IsAlreadyEndedMessage(message))
                {
                    return Ok(
                        new
                        {
                            Message = "Quiz sudah selesai",
                            AttemptId = request.Id
                        }
                    );
                }

                if (IsNullableIntMaterializeError(message))
                {
                    return BadRequest(
                        "Data attempt memiliki nilai NULL pada kolom angka. Jalankan update SQL TotalScore = 0 lalu perbaiki StartQuiz agar TotalScore selalu 0."
                    );
                }

                return BadRequest(
                    message
                );
            }
        }

        [Authorize]
        [HttpGet]
        [Route("api/QuizAttempt/Result/{attemptId}")]
        public IHttpActionResult Result(int attemptId)
        {
            var identity =
                User.Identity as ClaimsIdentity;

            if (identity == null)
                return Unauthorized();

            var role =
                identity.FindFirst(
                    ClaimTypes.Role
                )?.Value;

            if (
                role != "Student" &&
                role != "Teacher" &&
                role != "Admin"
            )
            {
                return Unauthorized();
            }

            if (attemptId <= 0)
                return BadRequest("AttemptId tidak valid");

            try
            {
                var result =
                    service.GetQuizResult(attemptId);

                return Ok(result);
            }
            catch (Exception ex)
            {
                var message =
                    ex.Message ?? "";

                if (IsNullableIntMaterializeError(message))
                {
                    return BadRequest(
                        "Data attempt memiliki nilai NULL pada kolom angka. Jalankan update SQL TotalScore = 0 lalu coba lagi."
                    );
                }

                return BadRequest(
                    message
                );
            }
        }

        [Authorize]
        [HttpGet]
        [Route("api/QuizAttempt/History")]
        public IHttpActionResult History()
        {
            var identity =
                User.Identity as ClaimsIdentity;

            if (identity == null)
                return Unauthorized();

            var role =
                identity.FindFirst(
                    ClaimTypes.Role
                )?.Value;

            if (role != "Student")
                return Unauthorized();

            var userId =
                GetLoginUserId();

            if (userId == null)
                return BadRequest("UserId tidak ditemukan di token");

            try
            {
                var result =
                    service.GetStudentQuizHistory(
                        userId.Value
                    );

                return Ok(result);
            }
            catch (Exception ex)
            {
                var message =
                    ex.Message ?? "";

                if (IsNullableIntMaterializeError(message))
                {
                    return BadRequest(
                        "Data history attempt memiliki nilai NULL pada kolom angka. Jalankan update SQL TotalScore = 0 lalu coba lagi."
                    );
                }

                return BadRequest(
                    message
                );
            }
        }

        private int? GetLoginUserId()
        {
            var identity =
                User.Identity as ClaimsIdentity;

            if (identity == null)
                return null;

            var userIdClaim =
                identity.FindFirst("UserId") ??
                identity.FindFirst("Id") ??
                identity.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
                return null;

            int userId;

            if (!int.TryParse(userIdClaim.Value, out userId))
                return null;

            return userId;
        }

        private bool IsAlreadyEndedMessage(string message)
        {
            var lower =
                (message ?? "").ToLower();

            return
                lower.Contains("sudah") ||
                lower.Contains("selesai") ||
                lower.Contains("already") ||
                lower.Contains("ended") ||
                lower.Contains("finished");
        }

        private bool IsNullableIntMaterializeError(string message)
        {
            var lower =
                (message ?? "").ToLower();

            return
                lower.Contains("system.int32") &&
                lower.Contains("materialized value is null");
        }
    }
}