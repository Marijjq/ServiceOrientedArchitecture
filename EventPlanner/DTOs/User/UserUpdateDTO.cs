using System.ComponentModel.DataAnnotations;

namespace EventPlanner.DTOs.User
{
    public class UserUpdateDTO
    {
        [MaxLength(50)]
        public string FirstName { get; set; }

        [MaxLength(50)]
        public string LastName { get; set; }

        [Phone]
        public string PhoneNumber { get; set; }

    }
}
