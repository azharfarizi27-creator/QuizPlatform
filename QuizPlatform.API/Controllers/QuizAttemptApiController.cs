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
        private readonly IQuizService service =
            new QuizService();

        [Authorize]
        [HttpPost]
        [Route("api/QuizAttempt/Start")]
        public IHttpActionResult Start(
            [FromBody] QuizAttempt attempt)
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

            var userIdClaim =
                identity.FindFirst("UserId") ??
                identity.FindFirst("Id") ??
                identity.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
                return BadRequest("UserId tidak ditemukan di token");

            attempt.UserId =
                int.Parse(userIdClaim.Value);

            try
            {
                var result =
                    service.StartQuiz(attempt);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Authorize]
        [HttpPost]
        [Route("api/QuizAttempt/End")]
        public IHttpActionResult End(
            [FromBody] QuizAttempt request)
        {
            var identity =
                User.Identity as ClaimsIdentity;

            var role =
                identity.FindFirst(
                    ClaimTypes.Role
                )?.Value;

            // hanya student
            if (role != "Student")
                return Unauthorized();

            service.EndQuiz(
                request.Id
            );

            return Ok(
                "Quiz selesai"
            );
        }

        [Authorize]
        [HttpGet]
        [Route("api/QuizAttempt/Result/{attemptId}")]
        public IHttpActionResult Result(
            int attemptId)
        {
            var identity =
                User.Identity as ClaimsIdentity;

            var role =
                identity.FindFirst(
                    ClaimTypes.Role
                )?.Value;

            // hanya student
            if (role != "Student")
                return Unauthorized();

            var result =
                service.GetQuizResult(
                    attemptId
                );

            return Ok(result);
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

            var userIdClaim =
                identity.FindFirst("UserId") ??
                identity.FindFirst("Id") ??
                identity.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
                return BadRequest("UserId tidak ditemukan di token");

            var userId =
                int.Parse(userIdClaim.Value);

            var result =
                service.GetStudentQuizHistory(userId);

            return Ok(result);
        }
    }
}