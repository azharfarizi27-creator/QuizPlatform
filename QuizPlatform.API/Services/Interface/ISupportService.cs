using QuizPlatform.API.Models.DTO;
using QuizPlatform.API.Models.Dtos;
using QuizPlatform.API.Models.Dtos.Support;
using System.Collections.Generic;

namespace QuizPlatform.API.Services.Interface
{
    public interface ISupportService
    {
        SupportTicketDto CreateTicket(
            int userId,
            CreateSupportTicketDto dto
        );

        List<SupportTicketDto> GetMyTickets(
            int userId
        );

        List<SupportTicketDto> GetAdminTickets();

        List<SupportMessageDto> GetTicketMessages(
            int loginUserId,
            int ticketId,
            bool isAdmin
        );

        SupportMessageDto SendMessage(
            int loginUserId,
            SendSupportMessageDto dto,
            bool isAdmin
        );

        void CloseTicket(
            int ticketId
        );
    }
}