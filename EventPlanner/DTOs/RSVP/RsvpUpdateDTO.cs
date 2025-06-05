using EventPlanner.Enums;
using System.ComponentModel.DataAnnotations;

namespace EventPlanner.DTOs.RSVP
{
    public class RsvpUpdateDTO
    {
        [Required]
        public RSVPStatus Status { get; set; }
        public string UserId { get; set; } 
        public int EventId { get; set; } 

    }
}
