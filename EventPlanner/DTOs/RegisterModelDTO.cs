using System.ComponentModel.DataAnnotations;

namespace EventPlanner.DTOs
{
    public class RegisterModelDTO
    {

        [Required, MaxLength(50)]
        public string Username { get; set; }

        [Required, EmailAddress, MaxLength(100)]
        public string Email { get; set; }

        [Required, MinLength(6)] // enforce minimum password length
        public string Password { get; set; }

        [Required, MaxLength(50)]
        public string FirstName { get; set; }

        [Required, MaxLength(50)]
        public string LastName { get; set; }

        [Phone]
        public string PhoneNumber { get; set; }

    }
}
