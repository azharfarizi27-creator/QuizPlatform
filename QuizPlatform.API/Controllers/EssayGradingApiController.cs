using QuizPlatform.API.Models.DTO;
using QuizPlatform.API.Models.Dtos;
using QuizPlatform.API.Services.Impl;
using QuizPlatform.API.Services.Interface;
using System;
using System.Security.Claims;
using System.Web.Http;

namespace QuizPlatform.API.Controllers
{
    public class EssayGradingApiController : ApiController
    {
        private readonly IEssayGradingService service =
            new EssayGradingService();

        [Authorize]
        [HttpGet]
        [Route("api/EssayGrading/Pending")]
        public IHttpActionResult Pending()
        {
            if (!IsTeacherOrAdmin())
                return Unauthorized();

            var result =
                service.GetPendingEssayAnswers();

            return Ok(result);
        }

        [Authorize]
        [HttpPut]
        [Route("api/EssayGrading/Grade")]
        public IHttpActionResult Grade(
            [FromBody] GradeEssayDto request
        )
        {
            if (!IsTeacherOrAdmin())
                return Unauthorized();

            try
            {
                service.GradeEssay(request);

                return Ok("Nilai essay berhasil disimpan");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private bool IsTeacherOrAdmin()
        {
            var identity =
                User.Identity as ClaimsIdentity;

            if (identity == null)
                return false;

            var role =
                identity.FindFirst(ClaimTypes.Role)?.Value;

            return role == "Teacher" ||
                   role == "Admin";
        }
    }
}