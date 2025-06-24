using System.ComponentModel.DataAnnotations;

namespace EventPlanner.DTOs.User
{
    public class UserCreateDTO
    {
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

        public string PhoneNumber { get; set; }
        public string Role { get; set; }


    }
}
