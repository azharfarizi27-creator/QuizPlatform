using QuizPlatform.API.Models.DTO;
using QuizPlatform.API.Models.Dtos;
using System.Collections.Generic;

namespace QuizPlatform.API.Services.Interface
{
    public interface IAntiCheatService
    {
        void CreateLog(CreateSuspiciousActivityDto request);

        List<AntiCheatLogDto> GetLogs();
    }
}