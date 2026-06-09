using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuizPlatform.API.Models.DTO
{
    public class UpdateProfileDto
    {

        public string FullName { get; set; }

        public string Username { get; set; }

        public string Email { get; set; }
    }
}