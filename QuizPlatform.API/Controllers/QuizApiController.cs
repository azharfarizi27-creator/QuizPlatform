using QuizPlatform.API.Models.Entity;
using QuizPlatform.API.Services.Impl;
using QuizPlatform.API.Services.Interface;
using System.Security.Claims;
using System.Web.Http;

namespace QuizPlatform.API.Controllers
{
    public class QuizApiController : ApiController
    {
        private readonly IQuizService service =
            new QuizService();

        [Authorize]
        [HttpGet]
        [Route("api/quiz/get")]
        public IHttpActionResult GetQuiz()
        {
            return Ok(service.GetAllQuizzes());
        }

        [Authorize]
        [HttpGet]
        [Route("api/Quiz/GetAll")]
        public IHttpActionResult GetAll()
        {
            return Ok(service.GetAllQuizzes());
        }

        [Authorize]
        [HttpPost]
        [Route("api/Quiz/Create")]
        public IHttpActionResult Create(
            [FromBody] Quiz quiz)
        {
            var identity = (ClaimsIdentity)User.Identity;

            var role = identity.FindFirst(ClaimTypes.Role)?.Value;

            // hanya Teacher dan Admin
            if (role != "Teacher" && role != "Admin")
                return Unauthorized();

            service.CreateQuiz(quiz);

            return Ok("Quiz berhasil dibuat");
        }
    }
}