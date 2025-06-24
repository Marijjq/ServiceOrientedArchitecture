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

            var result = await _userManager.CreateAsync(user, userDto.Password);
            if (!result.Succeeded)
            {
                var errorMessages = string.Join("; ", result.Errors.Select(e => e.Description));
                throw new ApplicationException($"Failed to create user: {errorMessages}");
            }

            await _userManager.AddToRoleAsync(user, userDto.Role);

            _cache.Remove("users");
            return _mapper.Map<UserDTO>(user);
        }
        public async Task<UserDTO> UpdateUserAsync(string userId, UserUpdateDTO userDto, string loggedInUserId)
        {
            var loggedInUser = await _userManager.FindByIdAsync(loggedInUserId);
            if (loggedInUser == null)
                throw new ArgumentException("Logged-in user not found.");

            var isAdmin = await _userManager.IsInRoleAsync(loggedInUser, "Admin");

            if (userId != loggedInUserId && !isAdmin)
                throw new UnauthorizedAccessException("You are not authorized to update this user.");

            var existingUser = await _userRepository.GetUserByIdAsync(userId);
            if (existingUser == null)
                throw new ArgumentException("User to update not found.");

            _mapper.Map(userDto, existingUser);
            await _userRepository.UpdateUserAsync(existingUser);

            // Role update logic...
            if (isAdmin && !string.IsNullOrEmpty(userDto.Role))
            {
                var currentRoles = await _userManager.GetRolesAsync(existingUser);
                var currentRole = currentRoles.FirstOrDefault();

                if (currentRole != userDto.Role)
                {
                    if (!string.IsNullOrEmpty(currentRole))
                        await _userManager.RemoveFromRoleAsync(existingUser, currentRole);

                    await _userManager.AddToRoleAsync(existingUser, userDto.Role);
                }
            }

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
