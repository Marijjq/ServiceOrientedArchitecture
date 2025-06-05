using EventPlanner.DTOs.User;
using EventPlanner.Models;

namespace EventPlanner.Services.Interfaces
{
    public interface IUserService
    {
        Task<UserDTO> GetUserByIdAsync(string userId);
        Task<IEnumerable<UserDTO>> GetAllUsersAsync();
        Task<UserDTO> CreateUserAsync(UserCreateDTO user);
        Task<UserDTO> UpdateUserAsync(string userId, UserUpdateDTO user, string loggedInUserId);
        Task DeleteUserAsync(string userId);

        //Additional
        Task<UserDTO> GetUserByEmailAsync(string username);
        Task<bool> IsAdminAsync(string userId);

    }
}
