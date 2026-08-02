using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuizPlatform.API.Models.DTO
{
    public class ChangeUserRoleDto
    {
        public int UserId { get; set; }

        public int RoleId { get; set; }
    }
}