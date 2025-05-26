using System.ComponentModel.DataAnnotations;

namespace EventPlanner.DTOs
{
    public class LoginModelDTO
    {
        [Required]
        public string EmailOrUsername { get; set; }

        [Required]
        public string Password { get; set; }

    }
}
