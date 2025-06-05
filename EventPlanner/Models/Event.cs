using EventPlanner.Enums;
using System.ComponentModel.DataAnnotations;

namespace EventPlanner.Models
{
    public class Event
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Title { get; set; }

        [MaxLength(1000)]
        public string Description { get; set; }

        [Required]
        public string Location { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        //Foreign key to User
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        //Foreign key to Category
        public int CategoryId { get; set; }
        public Category Category { get; set; }

        public List<RSVP> RSVPs { get; set; } = new();
        public List<Invite> Invites { get; set; } = new();

        public EventStatus Status { get; set; } = EventStatus.Upcoming; // Default status
        public int MaxParticipants { get; set; } // Maximum number of participants

    }
}
