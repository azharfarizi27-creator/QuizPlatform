using QuizPlatform.API.Models.Entity;
using QuizPlatform.API.Services.Impl;
using QuizPlatform.API.Services.Interface;
using System.Security.Claims;
using System.Web.Http;

namespace QuizPlatform.API.Controllers
{
    public class UserAnswerApiController
        : ApiController
    {
        private readonly IQuestionService questionService =
            new QuestionService();

        private readonly IQuizAttemptService attemptService =
            new QuizAttemptService();

        [Authorize]
        [HttpPost]
        [Route("api/UserAnswer/Submit")]
        public IHttpActionResult Submit(
            [FromBody] UserAnswer answer)
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

            attemptService.SubmitAnswer(
                answer
            );

            return Ok(
                "Jawaban berhasil disimpan"
            );
        }
    }
}