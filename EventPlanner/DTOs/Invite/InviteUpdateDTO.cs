using EventPlanner.Enums;
using System.ComponentModel.DataAnnotations;

namespace EventPlanner.DTOs.Invite
{
    public class InviteUpdateDTO
    {
        [Required]
        public InviteStatus Status { get; set; } // e.g., "Accepted", "Declined"
        public DateTime? RespondedAt { get; set; }
        public string? Message { get; set; }

    }
}
