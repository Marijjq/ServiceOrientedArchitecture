using System.ComponentModel.DataAnnotations;

namespace EventPlanner.Models
{
    public class RSVP
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(20)]
        public string Status { get; set; } // e.g., "Accepted", "Declined", "Maybe"

        [Required]
        public DateTime ResponseDate { get; set; }

        [Required]
        public int UserId { get; set; }
        public User User { get; set; }

        [Required]
        public int EventId { get; set; }
        public Event Event { get; set; }
    }
}
