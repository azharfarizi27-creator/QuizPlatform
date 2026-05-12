using QuizPlatform.API.Models.Entity;
using QuizPlatform.API.Services.Impl;
using QuizPlatform.API.Services.Interface;
using System.Security.Claims;
using System.Web.Http;

namespace QuizPlatform.API.Controllers
{
    public class QuestionOptionApiController
        : ApiController
    {
        private readonly IQuizService service =
            new QuizService();

        [HttpGet]
        [Route("api/QuestionOption/GetAll")]
        public IHttpActionResult GetAll()
        {
            return Ok(
                service.GetAllQuestionOptions()
            );
        }

        [Authorize]
        [HttpPost]
        [Route("api/QuestionOption/Create")]
        public IHttpActionResult Create(
            [FromBody] QuestionOption option)
        {
            var identity = (ClaimsIdentity)User.Identity;

            var role = identity.FindFirst(ClaimTypes.Role)?.Value;

            // hanya Teacher dan Admin
            if (role != "Teacher" && role != "Admin")
                return Unauthorized();

            service.CreateQuestionOption(option);

            return Ok("Option berhasil dibuat");
        }
    }
}