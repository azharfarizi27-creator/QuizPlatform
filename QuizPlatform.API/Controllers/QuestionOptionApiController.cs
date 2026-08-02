using QuizPlatform.API.Models.Entity;
using QuizPlatform.API.Services.Impl;
using QuizPlatform.API.Services.Interface;
using System.Linq;
using System.Security.Claims;
using System.Web.Http;

namespace QuizPlatform.API.Controllers
{
    public class QuestionOptionApiController
        : ApiController
    {
        private readonly IQuestionService service =
            new QuestionService();

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
            var identity =
                User.Identity as ClaimsIdentity;

            var role =
                identity.FindFirst(
                    ClaimTypes.Role
                )?.Value;

            // hanya Teacher dan Admin
            if (role != "Teacher" &&
                role != "Admin")
            {
                return Unauthorized();
            }

            service.CreateQuestionOption(
                option
            );

            return Ok(
                "Option berhasil dibuat"
            );
        }

        [Authorize]
        [HttpGet]
        [Route("api/QuestionOption/ByQuestion/{questionId}")]
        public IHttpActionResult ByQuestion(int questionId)
        {
            var result = service.GetAllQuestionOptions()
                .Where(x => x.QuestionId == questionId)
                .OrderBy(x => x.OrderNumber)
                .ToList();

            return Ok(result);
        }

        [Authorize]
        [HttpDelete]
        [Route("api/QuestionOption/Delete/{optionId}")]
        public IHttpActionResult Delete(int optionId)
        {
            var identity =
                User.Identity as ClaimsIdentity;

            var role =
                identity.FindFirst(
                    ClaimTypes.Role
                )?.Value;

            if (role != "Teacher" && role != "Admin")
                return Unauthorized();

            try
            {
                service.DeleteQuestionOption(optionId);

                return Ok("Jawaban berhasil dihapus");
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpPut]
        [Route("api/QuestionOption/Update")]
        public IHttpActionResult Update(
    [FromBody] QuestionOption option)
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

            service.UpdateQuestionOption(
                option
            );

            return Ok(
                "Jawaban berhasil diupdate"
            );
        }


        [Authorize]
        [HttpGet]
        [Route("api/QuestionOption/ByAttemptQuestion/{attemptId}/{questionId}")]
        public IHttpActionResult ByAttemptQuestion(int attemptId,int questionId)
        {
            var result =
                service.GetOptionsByAttemptQuestion(
                    attemptId,
                    questionId
                );

            return Ok(result);
        }
    }



}