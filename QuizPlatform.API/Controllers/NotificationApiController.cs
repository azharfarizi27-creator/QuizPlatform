using QuizPlatform.API.Services.Impl;
using QuizPlatform.API.Services.Interface;
using System;
using System.Security.Claims;
using System.Web.Http;

namespace QuizPlatform.API.Controllers
{
    [Authorize]
    public class NotificationApiController : ApiController
    {
        private readonly INotificationService service =
            new NotificationService();

        [HttpGet]
        [Route("api/Notification/Summary")]
        public IHttpActionResult GetSummary()
        {
            try
            {
                int userId =
                    GetCurrentUserId();

                var result =
                    service.GetSummary(userId);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [Route("api/Notification/Items")]
        public IHttpActionResult GetItems()
        {
            try
            {
                int userId =
                    GetCurrentUserId();

                var result =
                    service.GetItems(userId);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [Route("api/Notification/My")]
        public IHttpActionResult My()
        {
            try
            {
                int userId =
                    GetCurrentUserId();

                var result =
                    service.GetStudentNotifications(userId);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [Route("api/Notification/UnreadCount")]
        public IHttpActionResult UnreadCount()
        {
            try
            {
                int userId =
                    GetCurrentUserId();

                var count =
                    service.GetUnreadCount(userId);

                return Ok(new
                {
                    Count = count
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut]
        [Route("api/Notification/Read/{id}")]
        public IHttpActionResult Read(int id)
        {
            try
            {
                int userId =
                    GetCurrentUserId();

                service.MarkAsRead(id, userId);

                return Ok("Notifikasi sudah dibaca");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut]
        [Route("api/Notification/ReadAll")]
        public IHttpActionResult ReadAll()
        {
            try
            {
                int userId =
                    GetCurrentUserId();

                service.MarkAllAsRead(userId);

                return Ok("Semua notifikasi sudah dibaca");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private int GetCurrentUserId()
        {
            var identity =
                User.Identity as ClaimsIdentity;

            if (identity == null)
                throw new Exception("Token tidak valid");

            var idClaim =
                identity.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                identity.FindFirst("UserId")?.Value ??
                identity.FindFirst("userId")?.Value ??
                identity.FindFirst("Id")?.Value ??
                identity.FindFirst("id")?.Value;

            int userId;

            if (!int.TryParse(idClaim, out userId))
                throw new Exception("UserId tidak ditemukan di token");

            return userId;
        }
    }
}