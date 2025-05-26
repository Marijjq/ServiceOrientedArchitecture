using Microsoft.AspNetCore.Identity;

namespace EventPlanner.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        // Role is managed by IdentityRole, so no need to store string Role here
        public string PhoneNumber { get; set; }

    }
}
