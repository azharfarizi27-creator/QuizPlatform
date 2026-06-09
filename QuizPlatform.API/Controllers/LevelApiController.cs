using QuizPlatform.API.Models.Entity;
using QuizPlatform.API.Services.Impl;
using QuizPlatform.API.Services.Interface;
using System.Security.Claims;
using System.Web.Http;

namespace QuizPlatform.API.Controllers
{
    public class LevelApiController : ApiController
    {
        private readonly IQuizService service =
            new QuizService();

        [Authorize]
        [HttpGet]
        [Route("api/Level/GetAllLevels")]
        public IHttpActionResult GetAllLevels()
        {
            // semua user yang login boleh lihat
            var result = service.GetAllLevels();

            return Ok(result);
        }

        [Authorize]
        [HttpPost]
        [Route("api/Level/CreateLevel")]
        public IHttpActionResult CreateLevel(
            [FromBody] Level level)
        {
            var identity =
                User.Identity as ClaimsIdentity;

            var role =
                identity.FindFirst(
                    ClaimTypes.Role
                )?.Value;

            if (role != "Admin")
                return Unauthorized();

            if (level == null)
                return BadRequest("Data level wajib diisi");

            service.CreateLevel(level);

            return Ok("Level berhasil ditambahkan");
        }
    
    }
}