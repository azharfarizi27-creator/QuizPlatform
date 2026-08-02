using QuizPlatform.API.Models.DTO;
using QuizPlatform.API.Models.Dtos;
using System.Collections.Generic;

namespace QuizPlatform.API.Services.Interface
{
    public interface ITeacherReportService
    {
        List<TeacherQuizResultDto> GetTeacherAnalytics();

        TeacherStatsSummaryDto GetTeacherStatsSummary();

        List<TopStudentDto> GetTopStudents();

        List<QuestionAnalyticsDto> GetQuestionAnalytics();
    }
}