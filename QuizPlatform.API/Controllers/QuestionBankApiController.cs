using QuizPlatform.API.Models.Entity;
using QuizPlatform.API.Services.Impl;
using QuizPlatform.API.Services.Interface;
using System;
using System.Security.Claims;
using System.Web.Http;

namespace QuizPlatform.API.Controllers
{
    public class QuestionBankApiController : ApiController
    {
        private readonly IQuestionBankService service =
            new QuestionBankService();

        [Authorize]
        [HttpGet]
        [Route("api/QuestionBank/GetAll")]
        public IHttpActionResult GetAll()
        {
            var role =
                GetRole();

            if (role != "Teacher" && role != "Admin")
                return Unauthorized();

            var result =
                service.GetQuestionBanks();

            return Ok(result);
        }

        [Authorize]
        [HttpGet]
        [Route("api/QuestionBank/GetById/{id}")]
        public IHttpActionResult GetById(
            int id
        )
        {
            var role =
                GetRole();

            if (role != "Teacher" && role != "Admin")
                return Unauthorized();

            var result =
                service.GetQuestionBankById(id);

            if (result == null)
                return BadRequest("Bank soal tidak ditemukan");

            return Ok(result);
        }

        [Authorize]
        [HttpPost]
        [Route("api/QuestionBank/Create")]
        public IHttpActionResult Create(
            [FromBody] QuestionBank bank
        )
        {
            var role =
                GetRole();

            if (role != "Teacher" && role != "Admin")
                return Unauthorized();

            try
            {
                var userId =
                    GetLoginUserId();

                if (userId == null)
                    return BadRequest("UserId tidak ditemukan di token");

                bank.CreatedBy =
                    userId.Value;

                var result =
                    service.CreateQuestionBank(bank);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpPut]
        [Route("api/QuestionBank/Update")]
        public IHttpActionResult Update(
            [FromBody] QuestionBank bank
        )
        {
            var role =
                GetRole();

            if (role != "Teacher" && role != "Admin")
                return Unauthorized();

            try
            {
                service.UpdateQuestionBank(bank);

                return Ok("Bank soal berhasil diupdate");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpDelete]
        [Route("api/QuestionBank/Delete/{id}")]
        public IHttpActionResult Delete(
            int id
        )
        {
            var role =
                GetRole();

            if (role != "Teacher" && role != "Admin")
                return Unauthorized();

            try
            {
                service.DeleteQuestionBank(id);

                return Ok("Bank soal berhasil dihapus");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private string GetRole()
        {
            var identity =
                User.Identity as ClaimsIdentity;

            return identity
                .FindFirst(ClaimTypes.Role)
                ?.Value;
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

            return int.Parse(userIdClaim.Value);
        }
    }
}