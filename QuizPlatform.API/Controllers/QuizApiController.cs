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

        private readonly IActivityLogService activityLogService =
            new ActivityLogService();

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
            if (!IsTeacherOrAdmin())
                return Unauthorized();

            if (quiz == null)
                return BadRequest("Data quiz kosong");

            try
            {
                var userId =
                    GetLoginUserId();

                if (userId == null)
                    return BadRequest("UserId tidak ditemukan di token");

                quiz.CreatedBy =
                    userId.Value;

                var result =
                    service.CreateQuiz(quiz);

                activityLogService.CreateActivityLog(
                    userId,
                    "CREATE_QUIZ",
                    "Teacher/Admin membuat quiz: " + quiz.Title
                );

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
            int difficultyId
        )
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
            if (!IsTeacherOrAdmin())
                return Unauthorized();

            var userId =
                GetLoginUserId();

            if (userId == null)
                return BadRequest("UserId tidak ditemukan");

            var result =
                service.GetTeacherQuizzes(
                    userId.Value
                );

            return Ok(result);
        }

        [Authorize]
        [HttpGet]
        [Route("api/Quiz/ValidatePublish/{quizId}")]
        public IHttpActionResult ValidatePublish(int quizId)
        {
            if (!IsTeacherOrAdmin())
                return Unauthorized();

            try
            {
                var result =
                    service.ValidateQuizBeforePublish(quizId);

                return Ok(result);
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpPost]
        [Route("api/Quiz/Publish/{quizId}")]
        public IHttpActionResult Publish(int quizId)
        {
            if (!IsTeacherOrAdmin())
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
            if (!IsTeacherOrAdmin())
                return Unauthorized();

            try
            {
                var quiz =
                    service.GetQuizById(quizId);

                service.UnpublishQuiz(quizId);

                activityLogService.CreateActivityLog(
                    GetLoginUserId(),
                    "UNPUBLISH_QUIZ",
                    "Teacher/Admin unpublish quiz: " +
                    (quiz?.Title ?? "ID " + quizId)
                );

                return Ok("Quiz berhasil di-unpublish");
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpDelete]
        [Route("api/Quiz/Delete/{quizId}")]
        public IHttpActionResult Delete(int quizId)
        {
            if (!IsTeacherOrAdmin())
                return Unauthorized();

            try
            {
                var quiz =
                    service.GetQuizById(quizId);

                service.DeleteQuiz(quizId);

                activityLogService.CreateActivityLog(
                    GetLoginUserId(),
                    "DELETE_QUIZ",
                    "Teacher/Admin menghapus quiz: " +
                    (quiz?.Title ?? "ID " + quizId)
                );

                return Ok("Quiz berhasil dihapus");
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpPut]
        [Route("api/Quiz/Update")]
        public IHttpActionResult Update([FromBody] Quiz quiz)
        {
            if (!IsTeacherOrAdmin())
                return Unauthorized();

            if (quiz == null)
                return BadRequest("Data quiz kosong");

            try
            {
                service.UpdateQuiz(quiz);

                activityLogService.CreateActivityLog(
                    GetLoginUserId(),
                    "UPDATE_QUIZ",
                    "Teacher/Admin mengupdate quiz: " +
                    quiz.Title
                );

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
            var result =
                service.GetQuizById(quizId);

            if (result == null)
                return BadRequest("Quiz tidak ditemukan");

            return Ok(result);
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

        private int? GetLoginUserId()
        {
            var identity =
                User.Identity as ClaimsIdentity;

            if (identity == null)
                return null;

            var userIdClaim =
                identity.FindFirst("UserId") ??
                identity.FindFirst("Id") ??
                identity.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
                return null;

            int userId;

            if (!int.TryParse(userIdClaim.Value, out userId))
                return null;

            return userId;
        }
    }
}