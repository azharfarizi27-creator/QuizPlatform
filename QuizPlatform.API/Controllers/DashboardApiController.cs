using QuizPlatform.API.Models.Entity;
using QuizPlatform.API.Services.Impl;
using QuizPlatform.API.Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Http;

namespace QuizPlatform.API.Controllers
{
    public class DashboardApiController
            : ApiController
    {
        private readonly IQuizService service =
            new QuizService();

        [Authorize]
        [HttpGet]
        [Route("api/Dashboard/Stats")]
        public IHttpActionResult Stats()
        {
            var identity =
                (ClaimsIdentity)User.Identity;

            var role =
                identity.FindFirst(
                    ClaimTypes.Role
                )?.Value;

            // hanya admin
            if (role != "Admin")
                return Unauthorized();

            var result =
                service.GetDashboardStats();

            return Ok(result);
        }
    }
}