using QuizPlatform.API.Models.Entity;
using QuizPlatform.API.Services.Impl;
using QuizPlatform.API.Services.Interface;
using System.Security.Claims;
using System.Web.Http;

namespace QuizPlatform.API.Controllers
{
    public class UserApiController : ApiController
    {
        private readonly IQuizService service =
            new QuizService();

        [Authorize]
        [HttpGet]
        [Route("api/User/GetAllUsers")]
        public IHttpActionResult GetAllUsers()
        {
            var identity =
                (ClaimsIdentity)User.Identity;

            var role =
                identity.FindFirst(ClaimTypes.Role)?.Value;

            // hanya admin
            if (role != "Admin")
                return Unauthorized();

            var result = service.GetAllUser();

            return Ok(result);
        }

        [Authorize]
        [HttpPost]
        [Route("api/User/CreateUser")]
        public IHttpActionResult CreateUser(
            [FromBody] User newUser)
        {
            var identity =
                (ClaimsIdentity)User.Identity;

            var role =
                identity.FindFirst(ClaimTypes.Role)?.Value;

            // hanya admin
            if (role != "Admin")
                return Unauthorized();

            service.CreateUser(newUser);

            return Ok(
                "User berhasil ditambahkan"
            );
        }
    }
}