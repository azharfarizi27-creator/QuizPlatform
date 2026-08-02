using QuizPlatform.API.Services.Impl;
using QuizPlatform.API.Services.Interface;
using System;
using System.Security.Claims;
using System.Web.Http;

namespace QuizPlatform.API.Controllers
{
    public class TeacherApiController : ApiController
    {
        private readonly ITeacherReportService teacherReportService =
            new TeacherReportService();

        private readonly IAdminService adminService =
            new AdminService();

        [Authorize]
        [HttpGet]
        [Route("api/Teacher/Analytics")]
        public IHttpActionResult Analytics()
        {
            if (!IsTeacherOrAdmin())
                return Unauthorized();

            try
            {
                var result =
                    teacherReportService.GetTeacherAnalytics();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpGet]
        [Route("api/Teacher/DashboardStats")]
        public IHttpActionResult DashboardStats()
        {
            if (!IsTeacherOrAdmin())
                return Unauthorized();

            try
            {
                var result =
                    adminService.GetDashboardStats();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpGet]
        [Route("api/Teacher/StatsSummary")]
        public IHttpActionResult StatsSummary()
        {
            if (!IsTeacherOrAdmin())
                return Unauthorized();

            try
            {
                var result =
                    teacherReportService.GetTeacherStatsSummary();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpGet]
        [Route("api/Teacher/TopStudents")]
        public IHttpActionResult TopStudents()
        {
            if (!IsTeacherOrAdmin())
                return Unauthorized();

            try
            {
                var result =
                    teacherReportService.GetTopStudents();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpGet]
        [Route("api/Teacher/QuestionAnalytics")]
        public IHttpActionResult QuestionAnalytics()
        {
            if (!IsTeacherOrAdmin())
                return Unauthorized();

            try
            {
                var result =
                    teacherReportService.GetQuestionAnalytics();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private bool IsTeacherOrAdmin()
        {
            var identity =
                User.Identity as ClaimsIdentity;

            if (identity == null)
                return false;

            var role =
                identity.FindFirst(
                    ClaimTypes.Role
                )?.Value;

            return role == "Teacher" ||
                   role == "Admin";
        }
    }
}