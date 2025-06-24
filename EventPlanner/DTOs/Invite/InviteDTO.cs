using EventPlanner.Enums;
using EventPlanner.Services.Implementations;

namespace EventPlanner.DTOs.Invite
{
    public class InviteDTO
    {
        public int Id { get; set; }
        public InviteStatus Status { get; set; } 
        public DateTime SentAt { get; set; }
        public DateTime? RespondedAt { get; set; }
        public string? Message { get; set; }
        public string InviterId { get; set; }
        public string InviterName { get; set; } 
        public string InviteeId { get; set; }
        public string InviteeName { get; set; } 
        public int EventId { get; set; }
        public string EventTitle { get; set; } 
        public DateTime? ExpiresAt { get; set; }
    }
}
