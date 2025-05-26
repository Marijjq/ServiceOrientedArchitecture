using System.ComponentModel.DataAnnotations;

namespace EventPlanner.DTOs.Invite
{
    public class InviteCreateDTO
    {
        [Required]
        public int InviterId { get; set; }
        [Required]
        public int InviteeId { get; set; }
        [Required]
        public int EventId { get; set; }
        public string? Message { get; set; }
        public DateTime? ExpiresAt { get; set; }

    }
}
