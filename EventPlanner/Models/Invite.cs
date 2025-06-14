using EventPlanner.Enums;
using System.ComponentModel.DataAnnotations;

namespace EventPlanner.Models
{
    public class Invite
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public InviteStatus Status { get; set; } = InviteStatus.Pending;

        [Required]
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        public DateTime? RespondedAt { get; set; }

        public string? Message { get; set; }

        [Required]
        public string InviterId { get; set; }
        public ApplicationUser Inviter { get; set; }
        [Required]
        public string InviteeId { get; set; }
        public ApplicationUser Invitee { get; set; }



        [Required]
        public int EventId { get; set; }
        public virtual Event Event { get; set; }

        // Optional
        public DateTime? ExpiresAt { get; set; }

    }
}
