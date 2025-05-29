using EventPlanner.Enums;
using System.ComponentModel.DataAnnotations;

namespace EventPlanner.Models
{
    public class RSVP
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public RSVPStatus Status { get; set; } = RSVPStatus.Pending;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        [Required]
        public int UserId { get; set; }
        public User User { get; set; }

        [Required]
        public int EventId { get; set; }
        public Event Event { get; set; }
    }
}
