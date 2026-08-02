using QuizPlatform.API.Models.DTO;
using QuizPlatform.API.Models.Dtos;
using QuizPlatform.API.Models.Dtos.Support;
using QuizPlatform.API.Services.Impl;
using QuizPlatform.API.Services.Interface;
using System;
using System.Linq;
using System.Security.Claims;
using System.Web.Http;

namespace QuizPlatform.API.Controllers
{
    [Authorize]
    [RoutePrefix("api/Support")]
    public class SupportController : ApiController
    {
        private readonly ISupportService supportService;

        public SupportController()
        {
            supportService = new SupportService();
        }

        [HttpPost]
        [Route("CreateTicket")]
        public IHttpActionResult CreateTicket(
            CreateSupportTicketDto dto
        )
        {
            try
            {
                var userId =
                    GetLoginUserId();

                var result =
                    supportService.CreateTicket(
                        userId,
                        dto
                    );

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [Route("MyTickets")]
        public IHttpActionResult MyTickets()
        {
            try
            {
                var userId =
                    GetLoginUserId();

                var result =
                    supportService.GetMyTickets(
                        userId
                    );

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [Route("Admin/Tickets")]
        public IHttpActionResult AdminTickets()
        {
            try
            {
                if (!IsAdmin())
                    return Unauthorized();

                var result =
                    supportService.GetAdminTickets();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [Route("Messages/{ticketId:int}")]
        public IHttpActionResult Messages(
            int ticketId
        )
        {
            try
            {
                var userId =
                    GetLoginUserId();

                var result =
                    supportService.GetTicketMessages(
                        userId,
                        ticketId,
                        IsAdmin()
                    );

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Route("SendMessage")]
        public IHttpActionResult SendMessage(
            SendSupportMessageDto dto
        )
        {
            try
            {
                var userId =
                    GetLoginUserId();

                var result =
                    supportService.SendMessage(
                        userId,
                        dto,
                        false
                    );

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Route("Admin/Reply")]
        public IHttpActionResult AdminReply(
            SendSupportMessageDto dto
        )
        {
            try
            {
                if (!IsAdmin())
                    return Unauthorized();

                var userId =
                    GetLoginUserId();

                var result =
                    supportService.SendMessage(
                        userId,
                        dto,
                        true
                    );

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut]
        [Route("Admin/CloseTicket/{ticketId:int}")]
        public IHttpActionResult CloseTicket(
            int ticketId
        )
        {
            try
            {
                if (!IsAdmin())
                    return Unauthorized();

                supportService.CloseTicket(
                    ticketId
                );

                return Ok(new
                {
                    message = "Ticket bantuan berhasil ditutup"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private int GetLoginUserId()
        {
            var identity =
                User.Identity as ClaimsIdentity;

            var claim =
                identity?.FindFirst(ClaimTypes.NameIdentifier) ??
                identity?.FindFirst("UserId") ??
                identity?.FindFirst("userId") ??
                identity?.FindFirst("Id") ??
                identity?.Claims.FirstOrDefault(x =>
                    x.Type.ToLower().Contains("nameidentifier")
                );

            if (claim == null)
                throw new Exception("UserId tidak ditemukan dari token");

            return int.Parse(claim.Value);
        }

        private bool IsAdmin()
        {
            var identity =
                User.Identity as ClaimsIdentity;

            var role =
                identity?.FindFirst(ClaimTypes.Role)?.Value ??
                identity?.FindFirst("role")?.Value ??
                identity?.FindFirst("Role")?.Value ??
                "";

            return role.ToLower() == "admin";
        }
    }
}