using System.ComponentModel.DataAnnotations;

namespace EventPlanner.DTOs.User
{
    public class UserUpdateDTO
    {
        [Required]
        [MaxLength(100)]
        public string Username { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(255)]
        public string Email { get; set; }

        [MaxLength(50)]
        public string FirstName { get; set; }

        [MaxLength(50)]
        public string LastName { get; set; }

        [Phone]
        public string PhoneNumber { get; set; }

        [Required]
        public string Role { get; set; }
    }
}
