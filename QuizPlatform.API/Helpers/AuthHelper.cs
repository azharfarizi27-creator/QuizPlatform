using QuizPlatform.API.Models.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuizPlatform.API.Helpers
{
    public class AuthHelper
    {
        public static void CheckRole(User user, string roleName)
        {
            if (user == null)
                throw new Exception("Unauthorized");

            if (user.Role == null || user.Role.Name != roleName)
                throw new Exception("Access denied for this role");
        }
    }
}