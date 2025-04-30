using System.ComponentModel.DataAnnotations;

namespace EventPlanner.Models
{
    public class Invite
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(20)]
        public string Status { get; set; } 
    }
}
