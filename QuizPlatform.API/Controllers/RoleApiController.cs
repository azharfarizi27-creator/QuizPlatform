using QuizPlatform.API.Services.Impl;
using QuizPlatform.API.Services.Interface;
using System.Security.Claims;
using System.Web.Http;

namespace QuizPlatform.API.Controllers
{
    public class RoleApiController : ApiController
    {
        private readonly IAdminService service =
            new AdminService();

        [Authorize]
        [HttpGet]
        [Route("api/Role/GetAll")]
        public IHttpActionResult GetAll()
        {
            var identity =
                User.Identity as ClaimsIdentity;

            var role =
                identity.FindFirst(
                    ClaimTypes.Role
                )?.Value;

            if (role != "Admin")
                return Unauthorized();

            var result =
                service.GetAllRoles();

            return Ok(result);
        }
    }
}