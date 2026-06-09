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
        public IHttpActionResult Create([FromBody] Quiz quiz)
        {
            try
            {
                var identity = User.Identity as ClaimsIdentity;

                var role = identity.FindFirst(ClaimTypes.Role)?.Value;

                if (role != "Teacher" && role != "Admin")
                    return Unauthorized();

                var userIdClaim =
                    identity.FindFirst("UserId") ??
                    identity.FindFirst("Id") ??
                    identity.FindFirst(ClaimTypes.NameIdentifier);

                if (userIdClaim == null)
                    return BadRequest("UserId tidak ditemukan di token");

                quiz.CreatedBy = int.Parse(userIdClaim.Value);

                var result = service.CreateQuiz(quiz);

                return Ok(result);
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Authorize]
        [HttpGet]
        [Route("api/Quiz/Filter")]
        public IHttpActionResult Filter(
    int categoryId,
    int difficultyId)
        {
            var result =
                service.FilterQuizzes(
                    categoryId,
                    difficultyId
                );

            return Ok(result);
        }

        [Authorize]
        [HttpGet]
        [Route("api/Quiz/MyQuizzes")]
        public IHttpActionResult MyQuizzes()
        {
            var identity =
                User.Identity as ClaimsIdentity;

            var role =
                identity.FindFirst(ClaimTypes.Role)?.Value;

            if (role != "Teacher" && role != "Admin")
                return Unauthorized();

            var userIdClaim =
                identity.FindFirst("UserId") ??
                identity.FindFirst("Id") ??
                identity.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
                return BadRequest("UserId tidak ditemukan");

            var result =
                service.GetTeacherQuizzes(
                    int.Parse(userIdClaim.Value)
                );

            return Ok(result);
        }


        [Authorize]
        [HttpPost]
        [Route("api/Quiz/Publish/{quizId}")]
        public IHttpActionResult Publish(int quizId)
        {
            var identity = User.Identity as ClaimsIdentity;
            var role = identity.FindFirst(ClaimTypes.Role)?.Value;

            if (role != "Teacher" && role != "Admin")
                return Unauthorized();

            try
            {
                service.PublishQuiz(quizId);

                return Ok("Quiz berhasil dipublish");
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpPost]
        [Route("api/Quiz/Unpublish/{quizId}")]
        public IHttpActionResult Unpublish(int quizId)
        {
            var identity =
                User.Identity as ClaimsIdentity;

            var role =
                identity.FindFirst(
                    ClaimTypes.Role
                )?.Value;

            if (role != "Teacher" &&
                role != "Admin")
            {
                return Unauthorized();
            }

            service.UnpublishQuiz(quizId);

            return Ok(
                "Quiz berhasil di-unpublish"
            );
        }

        [Authorize]
        [HttpDelete]
        [Route("api/Quiz/Delete/{quizId}")]
        public IHttpActionResult Delete(int quizId)
        {
            var identity =
                User.Identity as ClaimsIdentity;

            var role =
                identity.FindFirst(
                    ClaimTypes.Role
                )?.Value;

            if (role != "Teacher" &&
                role != "Admin")
            {
                return Unauthorized();
            }

            service.DeleteQuiz(quizId);

            return Ok(
                "Quiz berhasil dihapus"
            );
        }

        [Authorize]
        [HttpPut]
        [Route("api/Quiz/Update")]
        public IHttpActionResult Update([FromBody] Quiz quiz)
        {
            var identity = User.Identity as ClaimsIdentity;
            var role = identity.FindFirst(ClaimTypes.Role)?.Value;

            if (role != "Teacher" && role != "Admin")
                return Unauthorized();

            try
            {
                service.UpdateQuiz(quiz);

                return Ok("Quiz berhasil diupdate");
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpGet]
        [Route("api/Quiz/GetById/{quizId}")]
        public IHttpActionResult GetById(int quizId)
        {
            var result = service.GetQuizById(quizId);

            if (result == null)
                return BadRequest("Quiz tidak ditemukan");

            return Ok(result);
        }
    }
}