using QuizPlatform.API.Services.Impl;
using QuizPlatform.API.Services.Interface;
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
    }
}