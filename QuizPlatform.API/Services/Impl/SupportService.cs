using QuizPlatform.API.Models;
using QuizPlatform.API.Models.DTO;
using QuizPlatform.API.Models.Dtos;
using QuizPlatform.API.Models.Dtos.Support;
using QuizPlatform.API.Models.Entity;
using QuizPlatform.API.Services.Base;
using QuizPlatform.API.Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QuizPlatform.API.Services.Impl
{
    public class SupportService : BaseServices, ISupportService
    {
        public SupportService()
            : base()
        {

        }

        public SupportTicketDto CreateTicket(
            int userId,
            CreateSupportTicketDto dto
        )
        {
            if (dto == null)
                throw new Exception("Data bantuan tidak boleh kosong");

            if (string.IsNullOrWhiteSpace(dto.Subject))
                throw new Exception("Subject wajib diisi");

            if (string.IsNullOrWhiteSpace(dto.Message))
                throw new Exception("Pesan wajib diisi");

            var user =
                context.Users
                    .FirstOrDefault(x =>
                        x.Id == userId
                    );

            if (user == null)
                throw new Exception("User tidak ditemukan");

            var ticket =
                new SupportTicket
                {
                    UserId = userId,
                    Subject = dto.Subject.Trim(),
                    Status = "Open",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

            context.SupportTickets.Add(ticket);
            context.SaveChanges();

            var message =
                new SupportMessage
                {
                    TicketId = ticket.Id,
                    SenderUserId = userId,
                    Message = dto.Message.Trim(),
                    IsAdmin = false,
                    CreatedAt = DateTime.Now
                };

            context.SupportMessages.Add(message);
            context.SaveChanges();

            return MapTicket(ticket);
        }

        public List<SupportTicketDto> GetMyTickets(
            int userId
        )
        {
            var tickets =
                context.SupportTickets
                    .Where(x =>
                        x.UserId == userId
                    )
                    .OrderByDescending(x =>
                        x.UpdatedAt ?? x.CreatedAt
                    )
                    .ToList();

            var result =
                tickets
                    .Select(x => MapTicket(x))
                    .ToList();

            return result;
        }

        public List<SupportTicketDto> GetAdminTickets()
        {
            var tickets =
                context.SupportTickets
                    .OrderByDescending(x =>
                        x.UpdatedAt ?? x.CreatedAt
                    )
                    .ToList();

            var result =
                tickets
                    .Select(x => MapTicket(x))
                    .ToList();

            return result;
        }

        public List<SupportMessageDto> GetTicketMessages(
            int loginUserId,
            int ticketId,
            bool isAdmin
        )
        {
            var ticket =
                context.SupportTickets
                    .FirstOrDefault(x =>
                        x.Id == ticketId
                    );

            if (ticket == null)
                throw new Exception("Ticket bantuan tidak ditemukan");

            if (!isAdmin && ticket.UserId != loginUserId)
                throw new Exception("Kamu tidak memiliki akses ke ticket ini");

            var messages =
                context.SupportMessages
                    .Where(x =>
                        x.TicketId == ticketId
                    )
                    .OrderBy(x =>
                        x.CreatedAt
                    )
                    .ToList();

            var result =
                messages
                    .Select(x => MapMessage(x))
                    .ToList();

            return result;
        }

        public SupportMessageDto SendMessage(
            int loginUserId,
            SendSupportMessageDto dto,
            bool isAdmin
        )
        {
            if (dto == null)
                throw new Exception("Data pesan tidak boleh kosong");

            if (dto.TicketId <= 0)
                throw new Exception("TicketId tidak valid");

            if (string.IsNullOrWhiteSpace(dto.Message))
                throw new Exception("Pesan wajib diisi");

            var ticket =
                context.SupportTickets
                    .FirstOrDefault(x =>
                        x.Id == dto.TicketId
                    );

            if (ticket == null)
                throw new Exception("Ticket bantuan tidak ditemukan");

            if (!isAdmin && ticket.UserId != loginUserId)
                throw new Exception("Kamu tidak memiliki akses ke ticket ini");

            if (ticket.Status == "Closed")
                throw new Exception("Ticket sudah ditutup");

            var message =
                new SupportMessage
                {
                    TicketId = dto.TicketId,
                    SenderUserId = loginUserId,
                    Message = dto.Message.Trim(),
                    IsAdmin = isAdmin,
                    CreatedAt = DateTime.Now
                };

            context.SupportMessages.Add(message);

            ticket.UpdatedAt = DateTime.Now;

            context.SaveChanges();

            return MapMessage(message);
        }

        public void CloseTicket(
            int ticketId
        )
        {
            var ticket =
                context.SupportTickets
                    .FirstOrDefault(x =>
                        x.Id == ticketId
                    );

            if (ticket == null)
                throw new Exception("Ticket bantuan tidak ditemukan");

            ticket.Status = "Closed";
            ticket.UpdatedAt = DateTime.Now;

            context.SaveChanges();
        }

        private SupportTicketDto MapTicket(
            SupportTicket ticket
        )
        {
            var user =
                context.Users
                    .FirstOrDefault(x =>
                        x.Id == ticket.UserId
                    );

            var lastMessage =
                context.SupportMessages
                    .Where(x =>
                        x.TicketId == ticket.Id
                    )
                    .OrderByDescending(x =>
                        x.CreatedAt
                    )
                    .FirstOrDefault();

            return new SupportTicketDto
            {
                Id = ticket.Id,

                UserId = ticket.UserId,

                FullName =
                    user != null
                        ? user.FullName
                        : "-",

                Username =
                    user != null
                        ? user.Username
                        : "-",

                Subject = ticket.Subject,

                Status = ticket.Status,

                LastMessage =
                    lastMessage != null
                        ? lastMessage.Message
                        : "-",

                CreatedAt = ticket.CreatedAt,

                UpdatedAt = ticket.UpdatedAt
            };
        }

        private SupportMessageDto MapMessage(
            SupportMessage message
        )
        {
            var user =
                context.Users
                    .FirstOrDefault(x =>
                        x.Id == message.SenderUserId
                    );

            return new SupportMessageDto
            {
                Id = message.Id,

                TicketId = message.TicketId,

                SenderUserId = message.SenderUserId,

                SenderName =
                    user != null
                        ? user.FullName
                        : "-",

                Message = message.Message,

                IsAdmin = message.IsAdmin,

                CreatedAt = message.CreatedAt
            };
        }
    }
}