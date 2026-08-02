using QuizPlatform.API.Models.Dtos;
using QuizPlatform.API.Models.Entity;
using QuizPlatform.API.Services.Impl;
using QuizPlatform.API.Services.Interface;
using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Http;

namespace QuizPlatform.API.Controllers
{
    public class QuestionApiController : ApiController
    {
        private readonly IQuestionService questionService =
            new QuestionService();

        private readonly IQuestionBankService questionBankService =
            new QuestionBankService();

        private readonly IQuizService quizService =
            new QuizService();

        

        [HttpGet]
        [Route("api/Question/GetAll")]
        public IHttpActionResult GetAll()
        {
            return Ok(
                questionService.GetAllQuestions()
            );
        }

        [Authorize]
        [HttpPost]
        [Route("api/Question/Create")]
        public IHttpActionResult Create(
            [FromBody] Question question
        )
        {
            var role =
                GetRole();

            if (role != "Teacher" && role != "Admin")
                return Unauthorized();

            try
            {
                questionService.CreateQuestion(question);

                return Ok("Question berhasil dibuat");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpGet]
        [Route("api/Question/ByQuiz/{quizId}")]
        public IHttpActionResult ByQuiz(
            int quizId
        )
        {
            var result =
                questionService.GetAllQuestions()
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
        public IHttpActionResult Update(
            [FromBody] Question question
        )
        {
            var role =
                GetRole();

            if (role != "Teacher" && role != "Admin")
                return Unauthorized();

            try
            {
                questionService.UpdateQuestion(question);

                return Ok("Soal berhasil diupdate");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpDelete]
        [Route("api/Question/Delete/{questionId}")]
        public IHttpActionResult Delete(
            int questionId
        )
        {
            var role =
                GetRole();

            if (role != "Teacher" && role != "Admin")
                return Unauthorized();

            try
            {
                questionService.DeleteQuestion(questionId);

                return Ok("Soal berhasil dihapus");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpPost]
        [Route("api/Question/UploadImage")]
        public IHttpActionResult UploadImage()
        {
            var role =
                GetRole();

            if (role != "Teacher" && role != "Admin")
                return Unauthorized();

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

        [Authorize]
        [HttpGet]
        [Route("api/Question/ByAttempt/{attemptId}")]
        public IHttpActionResult ByAttempt(
            int attemptId
        )
        {
            var result =
                questionService.GetQuestionsByAttempt(
                    attemptId
                );

            return Ok(result);
        }

        [Authorize]
        [HttpGet]
        [Route("api/Question/ByBank/{bankId}")]
        public IHttpActionResult ByBank(
            int bankId
        )
        {
            var role =
                GetRole();

            if (role != "Teacher" && role != "Admin")
                return Unauthorized();

            var result =
                questionBankService.GetQuestionsByBank(bankId);

            return Ok(result);
        }

        [Authorize]
        [HttpPost]
        [Route("api/Question/CopyToQuiz/{questionId}/{quizId}")]
        public IHttpActionResult CopyToQuiz(
            int questionId,
            int quizId
        )
        {
            var role =
                GetRole();

            if (role != "Teacher" && role != "Admin")
                return Unauthorized();

            try
            {
                var result =
                    questionBankService.CopyQuestionToQuiz(
                        questionId,
                        quizId
                    );

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpPost]
        [Route("api/Question/CopyRandomFromBank")]
        public IHttpActionResult CopyRandomFromBank(
            [FromBody] CopyRandomQuestionDto request
        )
        {
            var role =
                GetRole();

            if (role != "Teacher" && role != "Admin")
                return Unauthorized();

            try
            {
                var copiedCount =
                    questionBankService.CopyRandomQuestionsFromBankToQuiz(
                        request
                    );

                return Ok(
                    copiedCount + " soal berhasil ditambahkan ke quiz"
                );
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpGet]
        [Route("api/Quiz/Validate/{quizId}")]
        public IHttpActionResult Validate(
            int quizId
        )
        {
            var role =
                GetRole();

            if (role != "Teacher" && role != "Admin")
                return Unauthorized();

            try
            {
                var result =
                    quizService.ValidateQuizBeforePublish(
                        quizId
                    );

                return Ok(result);
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

            return identity?
                .FindFirst(ClaimTypes.Role)
                ?.Value;
        }


        [Authorize]
        [HttpPost]
        [Route("api/Question/ImportExcel")]
        public IHttpActionResult ImportExcel(
        [FromBody] ImportQuestionExcelRequestDto request
        )
        {
            var identity =
                User.Identity as ClaimsIdentity;

            if (identity == null)
                return Unauthorized();

            var role =
                identity.FindFirst(ClaimTypes.Role)?.Value;

            if (
                role != "Teacher" &&
                role != "Admin"
            )
            {
                return Unauthorized();
            }

            try
            {
                var importedCount =
                    questionService.ImportQuestionsFromExcel(request);

                return Ok(
                    new
                    {
                        Message =
                            "Import soal berhasil",

                        ImportedCount =
                            importedCount
                    }
                );
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}