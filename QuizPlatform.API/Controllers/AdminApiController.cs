using System;
using System.Security.Claims;
using System.Web.Http;
using QuizPlatform.API.Services.Interface;
using QuizPlatform.API.Services.Impl;
using QuizPlatform.API.Models.DTO;

namespace QuizPlatform.API.Controllers
{
    public class AdminApiController : ApiController
    {
        private readonly IAdminService service =
            new AdminService();

        private readonly IActivityLogService activityLogService =
            new ActivityLogService();

        [Authorize]
        [HttpGet]
        [Route("api/Admin/Users")]
        public IHttpActionResult Users()
        {
            if (!IsAdmin())
                return Unauthorized();

            var result =
                service.GetAllUser();

            return Ok(result);
        }

        [Authorize]
        [HttpPut]
        [Route("api/Admin/ChangeRole")]
        public IHttpActionResult ChangeRole(
            [FromBody] ChangeUserRoleDto request
        )
        {
            if (!IsAdmin())
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
        public IHttpActionResult DeleteUser(
            int userId
        )
        {
            if (!IsAdmin())
                return Unauthorized();

            try
            {
                service.DeleteUser(userId);

                return Ok("User berhasil dinonaktifkan");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpPut]
        [Route("api/Admin/ActivateUser/{userId}")]
        public IHttpActionResult ActivateUser(
            int userId
        )
        {
            if (!IsAdmin())
                return Unauthorized();

            try
            {
                service.ActivateUser(userId);

                return Ok("User berhasil diaktifkan kembali");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpGet]
        [Route("api/Admin/ActivityLogs")]
        public IHttpActionResult ActivityLogs()
        {
            if (!IsAdmin())
                return Unauthorized();

            var result =
                activityLogService.GetActivityLogs();

            return Ok(result);
        }

        private bool IsAdmin()
        {
            var identity =
                User.Identity as ClaimsIdentity;

            var role =
                identity?
                    .FindFirst(ClaimTypes.Role)
                    ?.Value;

            return role == "Admin";
        }
    }
}