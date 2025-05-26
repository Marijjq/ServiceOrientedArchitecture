using EventPlanner.Enums;
using System.ComponentModel.DataAnnotations;

namespace EventPlanner.DTOs.RSVP
{
    public class RsvpCreateDTO
    {
        [Required]
        public int EventId { get; set; }
        [Required]
        public RSVPStatus Status { get; set; } // e.g., "Going", "Maybe", "NotGoing"
        public int UserId { get; set; } // Added missing UserId property

    }
}
