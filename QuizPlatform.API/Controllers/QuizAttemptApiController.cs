using QuizPlatform.API.Models.Entity;
using QuizPlatform.API.Services.Impl;
using QuizPlatform.API.Services.Interface;
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

            var role =
                identity.FindFirst(
                    ClaimTypes.Role
                )?.Value;

            // hanya student
            if (role != "Student")
                return Unauthorized();

            var result =
                service.StartQuiz(
                    attempt
                );

            return Ok(result);
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
    }
}