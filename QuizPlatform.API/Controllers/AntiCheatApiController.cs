using QuizPlatform.API.Models.DTO;
using QuizPlatform.API.Models.Dtos;
using QuizPlatform.API.Services.Impl;
using QuizPlatform.API.Services.Interface;
using System;
using System.Security.Claims;
using System.Web.Http;

namespace QuizPlatform.API.Controllers
{
    public class AntiCheatApiController : ApiController
    {
        private readonly IAntiCheatService service =
            new AntiCheatService();

        [Authorize]
        [HttpPost]
        [Route("api/AntiCheat/Log")]
        public IHttpActionResult Log(
            [FromBody] CreateSuspiciousActivityDto request
        )
        {
            try
            {
                service.CreateLog(request);

                return Ok("Log anti-cheat berhasil disimpan");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpGet]
        [Route("api/AntiCheat/Logs")]
        public IHttpActionResult Logs()
        {
            var role =
                GetLoginRole();

            if (
                role != "Teacher" &&
                role != "Admin"
            )
            {
                return Unauthorized();
            }

            var result =
                service.GetLogs();

            return Ok(result);
        }

        private string GetLoginRole()
        {
            var identity =
                User.Identity as ClaimsIdentity;

            return identity?
                .FindFirst(ClaimTypes.Role)?
                .Value;
        }
    }
}