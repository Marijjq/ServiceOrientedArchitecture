using EventPlanner.Enums;
using EventPlanner.Services.Implementations;

namespace EventPlanner.DTOs.Invite
{
    public class InviteDTO
    {
        public int Id { get; set; }
        public InviteStatus Status { get; set; } // Enum as string for readability
        public DateTime SentAt { get; set; }
        public DateTime? RespondedAt { get; set; }
        public string? Message { get; set; }
        public int InviterId { get; set; }
        public string InviterName { get; set; } // Flattened from User
        public int InviteeId { get; set; }
        public string InviteeName { get; set; } // Flattened from User
        public int EventId { get; set; }
        public string EventTitle { get; set; } // Flattened from Event
        public DateTime? ExpiresAt { get; set; }
    }
}
