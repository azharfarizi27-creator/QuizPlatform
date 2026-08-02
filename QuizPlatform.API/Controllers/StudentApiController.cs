using QuizPlatform.API.Services.Impl;
using QuizPlatform.API.Services.Interface;
using System.Security.Claims;
using System.Web.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuizPlatform.API.Controllers
{
    public class StudentApiController : ApiController
    {

        private readonly IProfileService profileService = 
            new ProfileService();


        [Authorize]
        [HttpGet]
        [Route("api/Student/ProfileStats")]
        public IHttpActionResult ProfileStats()
        {
            var identity =
                User.Identity as ClaimsIdentity;

            var role =
                identity.FindFirst(
                    ClaimTypes.Role
                )?.Value;

            if (role != "Student")
                return Unauthorized();

            var userId =
                int.Parse(
                    identity.FindFirst("UserId")
                    ?.Value
                );

            var result =
                profileService.GetStudentProfileStats(
                    userId
                );

            return Ok(result);
        }


        [Authorize]
        [HttpGet]
        [Route("api/Student/Notifications")]
        public IHttpActionResult Notifications()
        {
            var identity =
                User.Identity as ClaimsIdentity;

            var userIdClaim =
                identity.FindFirst("UserId") ??
                identity.FindFirst("Id") ??
                identity.FindFirst(ClaimTypes.NameIdentifier);

            var userId =
                int.Parse(
                    userIdClaim.Value
                );
            var result =
                profileService.GetStudentNotifications(
                    userId
                );
            return Ok(result);
        }

    }
}