using QuizPlatform.API.Services.Impl;
using QuizPlatform.API.Services.Interface;
using System.Security.Claims;
using System.Web.Http;

namespace QuizPlatform.API.Controllers
{
    public class TeacherApiController
        : ApiController
    {
        private readonly IQuizService service =
            new QuizService();

        [Authorize]
        [HttpGet]
        [Route("api/Teacher/Analytics")]
        public IHttpActionResult Analytics()
        {
            var identity =
                User.Identity as ClaimsIdentity;

            var role =
                identity.FindFirst(
                    ClaimTypes.Role
                )?.Value;

            // hanya teacher & admin
            if (role != "Teacher" &&
                role != "Admin")
            {
                return Unauthorized();
            }

            var result =
                service.GetTeacherAnalytics();

            return Ok(result);
        }

        [Authorize]
        [HttpGet]
        [Route("api/Teacher/DashboardStats")]
        public IHttpActionResult DashboardStats()
        {
            var identity =
                User.Identity as ClaimsIdentity;

            var role =
                identity.FindFirst(
                    ClaimTypes.Role
                )?.Value;

            // hanya teacher & admin
            if (role != "Teacher" &&
                role != "Admin")
            {
                return Unauthorized();
            }

            var result =
                service.GetDashboardStats();

            return Ok(result);
        }



        [Authorize]
        [HttpGet]
        [Route("api/Teacher/TopStudents")]
        public IHttpActionResult TopStudents()
        {
            var identity =
                User.Identity as ClaimsIdentity;
            var role =
                identity.FindFirst(
                    ClaimTypes.Role
                )?.Value;
            // hanya teacher & admin
            if (role != "Teacher" &&
                role != "Admin")
            {
                return Unauthorized();
            }
            var result =
                service.GetTopStudents();
            return Ok(result);
        }
    

    [Authorize]
        [HttpGet]
        [Route("api/Teacher/StatsSummary")]
        public IHttpActionResult StatsSummary()
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

            var result =
                service.GetTeacherStatsSummary();

            return Ok(result);
        }
    }

}