using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Security.Claims;
using QuizPlatform.API.Services.Interface;
using QuizPlatform.API.Services.Impl;
using QuizPlatform.API.Models.DTO;

namespace QuizPlatform.API.Controllers
{
    public class AdminApiController : ApiController
    {

        private readonly IQuizService service =
            new QuizService();

        [Authorize]
        [HttpGet]
        [Route("api/Admin/Users")]
        public IHttpActionResult Users()
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
                service.GetAllUser();
            return Ok(result);
        }

        [Authorize]
        [HttpPut]
        [Route("api/Admin/ChangeRole")]
        public IHttpActionResult ChangeRole(
            [FromBody] ChangeUserRoleDto request)
        {
            var identity =
                User.Identity as ClaimsIdentity;
            var role =
                identity.FindFirst(
                    ClaimTypes.Role
                )?.Value;
            if (role != "Admin")
                return Unauthorized();

            try
            {
                service.ChangeUserRole(request);

                return Ok("Role User berhasil diubah");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

        [Authorize]
        [HttpDelete]
        [Route("api/Admin/DeleteUser/{userId}")]
        public IHttpActionResult DeleteUser(int userId)
        {
            var identity =
                User.Identity as ClaimsIdentity;

            var role =
                identity.FindFirst(
                    ClaimTypes.Role
                )?.Value;

            if (role != "Admin")
                return Unauthorized();

            try
            {
                service.DeleteUser(userId);

                return Ok("User berhasil dinonaktifkan");
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpPut]
        [Route("api/Admin/ActivateUser/{userId}")]
        public IHttpActionResult ActivateUser(int userId)
        {
            var identity =
                User.Identity as ClaimsIdentity;

            var role =
                identity.FindFirst(
                    ClaimTypes.Role
                )?.Value;

            if (role != "Admin")
                return Unauthorized();

            try
            {
                service.ActivateUser(userId);

                return Ok("User berhasil diaktifkan kembali");
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

    }
}