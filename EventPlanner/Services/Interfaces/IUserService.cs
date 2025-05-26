using EventPlanner.DTOs.User;
using EventPlanner.Models;

namespace EventPlanner.Services.Interfaces
{
    public interface IUserService
    {
        Task<UserDTO> GetUserByIdAsync(int userId);
        Task<IEnumerable<UserDTO>> GetAllUsersAsync();
        Task<UserDTO> CreateUserAsync(UserCreateDTO user);
        Task<UserDTO> UpdateUserAsync(int userId, UserUpdateDTO user);
        Task DeleteUserAsync(int userId);

        //Additional
        Task<UserDTO> GetUserByEmailAsync(string username);
    }
}
