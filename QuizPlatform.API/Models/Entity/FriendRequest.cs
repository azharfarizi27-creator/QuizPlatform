using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace QuizPlatform.API.Models.Entity
{
   
        [Table("FriendRequests")]
        public class FriendRequest
        {
            [Key]
            public int Id { get; set; }

            public int RequesterId { get; set; }

            public int ReceiverId { get; set; }

            public string Status { get; set; }

            public DateTime CreatedAt { get; set; }

            public DateTime? RespondedAt { get; set; }
        }
    
}