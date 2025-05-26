using System.ComponentModel.DataAnnotations;

namespace EventPlanner.DTOs
{
    public class UserRoleAssignmentDTO
    {
        [Required]
        public string UserId { get; set; }

        [Required]
        public string RoleName { get; set; }

    }
}
