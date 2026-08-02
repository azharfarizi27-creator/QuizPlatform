using QuizPlatform.API.Models.Entity;
using QuizPlatform.API.Services.Impl;
using QuizPlatform.API.Services.Interface;
using System.Security.Claims;
using System.Web.Http;

namespace QuizPlatform.API.Controllers
{
    public class LevelApiController : ApiController
    {
        private readonly ILookupService service =
            new LookupService();

        [Authorize]
        [HttpGet]
        [Route("api/Level/GetAllLevels")]
        public IHttpActionResult GetAllLevels()
        {
            var result =
                service.GetAllLevels();

            return Ok(result);
        }

        [Authorize]
        [HttpPost]
        [Route("api/Level/CreateLevel")]
        public IHttpActionResult CreateLevel(
            [FromBody] Level level
        )
        {
            var identity =
                User.Identity as ClaimsIdentity;

            var role =
                identity?
                    .FindFirst(ClaimTypes.Role)
                    ?.Value;

            if (role != "Admin")
                return Unauthorized();

            if (level == null)
                return BadRequest("Data level wajib diisi");

            try
            {
                service.CreateLevel(level);

                return Ok("Level berhasil ditambahkan");
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}