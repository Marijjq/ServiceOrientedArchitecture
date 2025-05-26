using System.ComponentModel.DataAnnotations;

namespace EventPlanner.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(50)]
        public string Username { get; set; }

        [Required, EmailAddress, MaxLength(100)]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        [Required, MaxLength(50)]
        public string FirstName { get; set; }

        [Required, MaxLength(50)]
        public string LastName { get; set; }

        [Required, MaxLength(50)]
        public string Role { get; set; } = "User"; // Default role is User

        [Phone]
        public string PhoneNumber { get; set; }

        public List<Event> Events { get; set; } = new();
        public List<RSVP> RSVPs { get; set; } = new();
        public List<Invite> Invites { get; set; } = new();
    }
}
