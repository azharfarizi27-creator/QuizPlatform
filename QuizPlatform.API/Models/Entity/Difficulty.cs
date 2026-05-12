using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuizPlatform.API.Models.Entity
{
    [Table("Difficulties")]
    public class Difficulty
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }
    }
}