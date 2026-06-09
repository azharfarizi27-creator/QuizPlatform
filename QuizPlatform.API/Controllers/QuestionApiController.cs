using QuizPlatform.API.Models.Entity;
using QuizPlatform.API.Services.Impl;
using QuizPlatform.API.Services.Interface;
using System.Linq;
using System.Security.Claims;
using System.Web.Http;
using System;
using System.IO;
using System.Net.Http;
using System.Web;

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

            service.CreateQuestion(
                question
            );

            return Ok(
                "Question berhasil dibuat"
            );
        }
        [Authorize]
        [HttpGet]
        [Route("api/Question/ByQuiz/{quizId}")]
        public IHttpActionResult ByQuiz(
    int quizId)
        {
            var result =
                service.GetAllQuestions()
                .Where(x =>
                    x.QuizId == quizId
                )
                .OrderBy(x =>
                    x.OrderNumber
                )
                .ToList();

            return Ok(result);
        }

        [Authorize]
        [HttpPut]
        [Route("api/Question/Update")]
        public IHttpActionResult Update([FromBody] Question question)
        {
            var identity = User.Identity as ClaimsIdentity;
            var role = identity.FindFirst(ClaimTypes.Role)?.Value;

            if (role != "Teacher" && role != "Admin")
                return Unauthorized();

            try
            {
                service.UpdateQuestion(question);
                return Ok("Soal berhasil diupdate");
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpDelete]
        [Route("api/Question/Delete/{questionId}")]
        public IHttpActionResult Delete(int questionId)
        {
            var identity = User.Identity as ClaimsIdentity;
            var role = identity.FindFirst(ClaimTypes.Role)?.Value;

            if (role != "Teacher" && role != "Admin")
                return Unauthorized();

            try
            {
                service.DeleteQuestion(questionId);
                return Ok("Soal berhasil dihapus");
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpPost]
        [Route("api/Question/UploadImage")]
        public IHttpActionResult UploadImage()
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

            var request =
                HttpContext.Current.Request;

            if (request.Files.Count == 0)
                return BadRequest("File tidak ditemukan");

            var file =
                request.Files[0];

            if (file == null ||
                file.ContentLength == 0)
            {
                return BadRequest("File kosong");
            }

            var extension =
                Path.GetExtension(file.FileName)
                .ToLower();

            var allowedExtensions =
                new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

            if (!allowedExtensions.Contains(extension))
                return BadRequest("Format gambar tidak valid");

            var folderPath =
                HttpContext.Current.Server.MapPath(
                    "~/Uploads/Questions"
                );

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            var fileName =
                Guid.NewGuid().ToString() + extension;

            var fullPath =
                Path.Combine(folderPath, fileName);

            file.SaveAs(fullPath);

            var imageUrl =
                "/Uploads/Questions/" + fileName;

            return Ok(new
            {
                imageUrl = imageUrl
            });
        }

    }
}