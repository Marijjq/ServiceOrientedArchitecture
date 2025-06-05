using EventPlanner.Models;

namespace EventPlanner.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<ApplicationUser> GetUserByIdAsync(string userId);
        Task<IEnumerable<ApplicationUser>> GetAllUsersAsync();
        Task AddUserAsync(ApplicationUser user);
        Task UpdateUserAsync(ApplicationUser user);
        Task DeleteUserAsync(string userId);

        //Additional
        Task<ApplicationUser> GetUserByEmailAsync(string username);
    }
}
