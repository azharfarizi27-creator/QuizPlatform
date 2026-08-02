using QuizPlatform.API.Models.Dtos;
using QuizPlatform.API.Models.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuizPlatform.API.Services.Interface
{
    public interface IActivityLogService
    {

        void CreateActivityLog(
           int? userId,
           string action,
           string description
       );

        List<ActivityLogDto> GetActivityLogs();
    }
}