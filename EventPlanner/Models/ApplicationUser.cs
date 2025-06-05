using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace EventPlanner.Models
{
    public class ApplicationUser : IdentityUser
    {
        [MaxLength(50)]
        public string? FirstName { get; set; }

        [MaxLength(50)]
        public string? LastName { get; set; }

        [Phone]
        public override string PhoneNumber { get; set; }

        public List<Event> Events { get; set; } = new();
        public List<RSVP> RSVPs { get; set; } = new();
        public List<Invite> Invites { get; set; } = new();

    }
}
