using QuizPlatform.API.Models.Entity;
using QuizPlatform.API.Services.Impl;
using QuizPlatform.API.Services.Interface;
using System.Security.Claims;
using System.Web.Http;

namespace QuizPlatform.API.Controllers
{
    public class QuestionApiController : ApiController
    {
        private readonly IQuizService service =
            new QuizService();

        [HttpGet]
        [Route("api/Question/GetAll")]
        public IHttpActionResult GetAll()
        {
            return Ok(
                service.GetAllQuestions()
            );
        }

        [Authorize]
        [HttpPost]
        [Route("api/Question/Create")]
        public IHttpActionResult Create(
            [FromBody] Question question)
        {
            var identity = (ClaimsIdentity)User.Identity;

            var role = identity.FindFirst(ClaimTypes.Role)?.Value;

            if (role != "Teacher" && role != "Admin")
                return Unauthorized();

            service.CreateQuestion(question);

            return Ok("Question berhasil dibuat");
        }
    }
}