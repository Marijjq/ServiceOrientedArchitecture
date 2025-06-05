using EventPlanner.Enums;

namespace EventPlanner.DTOs.RSVP
{
    public class RsvpDTO
    {
        public int Id { get; set; }
        public RSVPStatus Status { get; set; } // Enum as string for readability
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; } // Flattened from User
        public int EventId { get; set; }
        public string EventTitle { get; set; } // Flattened from Event

    }
}
