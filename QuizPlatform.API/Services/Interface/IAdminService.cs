using QuizPlatform.API.Models.DTO;
using QuizPlatform.API.Models.Dtos;
using QuizPlatform.API.Models.Entity;
using System.Collections.Generic;

namespace QuizPlatform.API.Services.Interface
{
    public interface IAdminService
    {
        List<User> GetAllUser();

        void CreateUser(User user);

        DashboardStatsDto GetDashboardStats();

        List<AdminUserDto> GetAdminUsers();

        void ChangeUserRole(ChangeUserRoleDto request);

        void DeleteUser(int userId);

        void ActivateUser(int userId);

        List<Role> GetAllRoles();
    }
}