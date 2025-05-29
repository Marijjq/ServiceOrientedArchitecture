using AutoMapper;
using EventPlanner.DTOs.User;
using EventPlanner.Models;
using EventPlanner.Repositories.Interfaces;
using EventPlanner.Services.Interfaces;

namespace EventPlanner.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        public UserService(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper=mapper;
        }
        public async Task<IEnumerable<UserDTO>> GetAllUsersAsync()
        {
            var users = await _userRepository.GetAllUsersAsync();
            return _mapper.Map<IEnumerable<UserDTO>>(users);
        }
        public async Task<UserDTO> GetUserByIdAsync(int userId)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            return _mapper.Map<UserDTO>(user);
        }
        public async Task<UserDTO> CreateUserAsync(UserCreateDTO userDto)
        {
            var existingUser = await _userRepository.GetUserByEmailAsync(userDto.Email);
            if (existingUser != null)
            {
                throw new ArgumentException("User with this email already exists.");
            }
            var user = _mapper.Map<User>(userDto);
            await _userRepository.AddUserAsync(user);
            return _mapper.Map<UserDTO>(user);
        }
        public async Task<UserDTO> UpdateUserAsync(int userId, UserUpdateDTO userDto, int loggedInUserId)
        {
            if (userId != loggedInUserId)
            {
                throw new UnauthorizedAccessException("You can only update your own user information.");
            }

            var existingUser = await _userRepository.GetUserByIdAsync(userId);
            if (existingUser == null)
            {
                throw new ArgumentException("User not found.");
            }

            // Map changes from DTO to the existing user entity
            _mapper.Map(userDto, existingUser);

            await _userRepository.UpdateUserAsync(existingUser);

            return _mapper.Map<UserDTO>(existingUser);
        }

        public async Task DeleteUserAsync(int userId)
        {
            await _userRepository.DeleteUserAsync(userId);
        }


        //Additional
        public async Task<UserDTO> GetUserByEmailAsync(string username)
        {
            var user = await _userRepository.GetUserByEmailAsync(username);
            return _mapper.Map<UserDTO>(user);
        }
    }
}
