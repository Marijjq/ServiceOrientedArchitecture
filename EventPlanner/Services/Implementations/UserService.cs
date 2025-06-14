using AutoMapper;
using EventPlanner.DTOs.User;
using EventPlanner.Models;
using EventPlanner.Repositories.Interfaces;
using EventPlanner.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;

namespace EventPlanner.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMemoryCache _cache;

        public UserService(IUserRepository userRepository, IMapper mapper, 
            UserManager<ApplicationUser> userManager, IMemoryCache memoryCache)
        {
            _userRepository = userRepository;
            _mapper=mapper;
            _userManager=userManager;
            _cache = memoryCache;
        }
        public async Task<IEnumerable<UserDTO>> GetAllUsersAsync()
        {
            if (_cache.TryGetValue("users", out IEnumerable<UserDTO> cachedUsers))
            {
                return cachedUsers;
            }

            var users = await _userRepository.GetAllUsersAsync();
            var userDtos = _mapper.Map<IEnumerable<UserDTO>>(users);

            foreach (var userDto in userDtos)
            {
                var user = await _userManager.FindByIdAsync(userDto.Id);
                var roles = await _userManager.GetRolesAsync(user);
                userDto.Role = roles.FirstOrDefault() ?? "User";
            }

            // Store in cache for 10 minutes
            _cache.Set("users", userDtos, new MemoryCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromMinutes(10)
            });

            return userDtos;
        }

        public async Task<UserDTO> GetUserByIdAsync(string userId)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null) return null;

            var userDto = _mapper.Map<UserDTO>(user);

            var roles = await _userManager.GetRolesAsync(user);
            userDto.Role = roles.FirstOrDefault() ?? "User";

            return userDto;
        }

        public async Task<UserDTO> CreateUserAsync(UserCreateDTO userDto)
        {
            var existingUser = await _userRepository.GetUserByEmailAsync(userDto.Email);
            if (existingUser != null)
            {
                throw new ArgumentException("User with this email already exists.");
            }
            var user = _mapper.Map<ApplicationUser>(userDto);
            await _userRepository.AddUserAsync(user);
            _cache.Remove("users");
            return _mapper.Map<UserDTO>(user);

        }
        public async Task<UserDTO> UpdateUserAsync(string userId, UserUpdateDTO userDto, string loggedInUserId)
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
            _cache.Remove("users");
            return _mapper.Map<UserDTO>(existingUser);
        }

        public async Task DeleteUserAsync(string userId)
        {
            _cache.Remove("users");
            await _userRepository.DeleteUserAsync(userId);
        }


        //Additional
        public async Task<UserDTO> GetUserByEmailAsync(string username)
        {
            var user = await _userRepository.GetUserByEmailAsync(username);
            return _mapper.Map<UserDTO>(user);
        }
        public async Task<bool> IsAdminAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            return await _userManager.IsInRoleAsync(user, "Admin");
        }

    }
}
