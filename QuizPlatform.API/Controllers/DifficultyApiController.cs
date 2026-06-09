using QuizPlatform.API.Models.Entity;
using QuizPlatform.API.Services.Impl;
using QuizPlatform.API.Services.Interface;
using System.Security.Claims;
using System.Web.Http;

namespace QuizPlatform.API.Controllers
{
    public class DifficultyApiController : ApiController
    {
        private readonly IQuizService service =
            new QuizService();

        [Authorize]
        [HttpGet]
        [Route("api/Difficulty/GetAll")]
        public IHttpActionResult GetAll()
        {
            var result =
                service.GetAllDifficulties();

            return Ok(result);
        }

        [Authorize]
        [HttpPost]
        [Route("api/Difficulty/Create")]
        public IHttpActionResult Create(
    [FromBody] Difficulty difficulty)
        {
            var identity =
                User.Identity as ClaimsIdentity;

            var role =
                identity.FindFirst(
                    ClaimTypes.Role
                )?.Value;

            if (role != "Admin")
                return Unauthorized();

            if (difficulty == null)
                return BadRequest("Data difficulty wajib diisi");

            service.CreateDifficulty(difficulty);

            return Ok("Difficulty berhasil ditambahkan");
        }
    }
}