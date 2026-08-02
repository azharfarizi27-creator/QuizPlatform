using QuizPlatform.API.Models.DTO;
using QuizPlatform.API.Services.Impl;
using QuizPlatform.API.Services.Interface;
using System.Security.Claims;
using System.Web.Http;

namespace QuizPlatform.API.Controllers
{
    public class LeaderboardApiController : ApiController
    {
        private readonly IQuizAttemptService service =
            new QuizAttemptService();


        [Authorize]
        [HttpPost]
        [Route("api/Leaderboard/Create")]
        public IHttpActionResult Create(
            [FromBody] LeaderboardRequest request)
        {
            var identity =
                (ClaimsIdentity)User.Identity;

            var role =
                identity.FindFirst(
                    ClaimTypes.Role
                )?.Value;

            if (role != "Student")
                return Unauthorized();

            if (request == null)
                return BadRequest("Request kosong");

            service.CreateLeaderboard(
                request.AttemptId
            );

            return Ok(
                "Leaderboard berhasil dibuat"
            );
        }

        [Authorize]
        [HttpGet]
        [Route("api/Leaderboard/Get/{quizId}")]
        public IHttpActionResult Get(
            int quizId)
        {
            var result =
                service.GetLeaderboard(
                    quizId
                );

            return Ok(result);
        }

        [Authorize]
        [HttpPost]
        [Route("api/Leaderboard/UpdateRank/{quizId}")]
        public IHttpActionResult UpdateRank(
            int quizId)
        {
            var identity =
                (ClaimsIdentity)User.Identity;

            var role =
                identity.FindFirst(
                    ClaimTypes.Role
                )?.Value;

            if (role != "Admin")
                return Unauthorized();

            service.UpdateRanking(
                quizId
            );

            return Ok(
                "Ranking updated"
            );
        }
    }
}