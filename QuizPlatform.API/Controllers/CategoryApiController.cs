using QuizPlatform.API.Models.Entity;
using QuizPlatform.API.Services.Impl;
using QuizPlatform.API.Services.Interface;
using System.Security.Claims;
using System.Web.Http;

namespace QuizPlatform.API.Controllers
{
    public class CategoryApiController : ApiController
    {
        private readonly IQuizService service =
            new QuizService();

        [HttpGet]
        [Route("api/Category/GetAllCategories")]
        public IHttpActionResult GetAllCategories()
        {
            var result = service.GetAllCategories();

            return Ok(result);
        }

        [Authorize]
        [HttpPost]
        [Route("api/Category/CreateCategory")]
        public IHttpActionResult CreateCategory(
            [FromBody] Category category)
        {
            var identity =
                User.Identity as ClaimsIdentity;

            var role =
                identity.FindFirst(
                    ClaimTypes.Role
                )?.Value;

            if (role != "Admin")
                return Unauthorized();

            if (category == null)
            {
                return BadRequest(
                    "Data category wajib diisi"
                );
            }

            service.CreateCategory(category);

            return Ok(
                "Category berhasil ditambahkan"
            );
        }
   
    }
}