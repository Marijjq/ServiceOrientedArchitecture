using AutoMapper;
using EventPlanner.DTOs.User;
using EventPlanner.Models;
using EventPlanner.Repositories.Interfaces;
using EventPlanner.Services.Implementations;
using Microsoft.AspNetCore.Identity;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventPlannerTest.Services
{
    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
        private readonly UserService _userService;

        public UserServiceTests()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _mockMapper = new Mock<IMapper>();
            _mockUserManager = new Mock<UserManager<ApplicationUser>>(
                new Mock<IUserStore<ApplicationUser>>().Object,
                null, null, null, null, null, null, null, null
            );
            _userService = new UserService(_mockUserRepository.Object, _mockMapper.Object, _mockUserManager.Object);
        }     

        [Fact]
        public async Task GetAllUsersAsync_ReturnsMappedUsers()
        {
            var users = new List<ApplicationUser>
            {
                new ApplicationUser { Id = "1" },
                new ApplicationUser { Id = "2" }
            };
            var userDTOs = new List<UserDTO>
            {
                new UserDTO { Id = "1" },
                new UserDTO { Id = "2" }
            };

            _mockUserRepository.Setup(r => r.GetAllUsersAsync()).ReturnsAsync(users);
            _mockMapper.Setup(m => m.Map<IEnumerable<UserDTO>>(users)).Returns(userDTOs);

            // Mock UserManager to return a non-null list of roles for each user
            _mockUserManager
                .Setup(m => m.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync((string id) => users.FirstOrDefault(u => u.Id == id));
            _mockUserManager
                .Setup(m => m.GetRolesAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(new List<string> { "User" });

            var result = await _userService.GetAllUsersAsync();

            Assert.Equal(userDTOs, result);
            _mockUserRepository.Verify(r => r.GetAllUsersAsync(), Times.Once);
            _mockMapper.Verify(m => m.Map<IEnumerable<UserDTO>>(users), Times.Once);
        }

        [Fact]
        public async Task GetUserByIdAsync_ReturnsMappedUser()
        {
            var user = new ApplicationUser();
            var userDto = new UserDTO();

            _mockUserRepository.Setup(r => r.GetUserByIdAsync("1")).ReturnsAsync(user);
            _mockMapper.Setup(m => m.Map<UserDTO>(user)).Returns(userDto);
            // Fix: Mock UserManager to return a non-null list of roles
            _mockUserManager
                .Setup(m => m.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { "User" });

            var result = await _userService.GetUserByIdAsync("1");

            Assert.Equal(userDto, result);
        }

        [Fact]
        public async Task CreateUserAsync_WhenEmailExists_ThrowsException()
        {
            var userCreateDto = new UserCreateDTO { Email = "existing@example.com" };
            _mockUserRepository.Setup(r => r.GetUserByEmailAsync(userCreateDto.Email)).ReturnsAsync(new ApplicationUser());

            await Assert.ThrowsAsync<ArgumentException>(() => _userService.CreateUserAsync(userCreateDto));
        }

        [Fact]
        public async Task CreateUserAsync_ReturnsCreatedUser()
        {
            var userCreateDto = new UserCreateDTO { Email = "new@example.com" };
            var user = new ApplicationUser();
            var userDto = new UserDTO();

            _mockUserRepository.Setup(r => r.GetUserByEmailAsync(userCreateDto.Email)).ReturnsAsync((ApplicationUser)null);
            _mockMapper.Setup(m => m.Map<ApplicationUser>(userCreateDto)).Returns(user);
            _mockUserRepository.Setup(r => r.AddUserAsync(user)).Returns(Task.CompletedTask);
            _mockMapper.Setup(m => m.Map<UserDTO>(user)).Returns(userDto);

            var result = await _userService.CreateUserAsync(userCreateDto);

            Assert.Equal(userDto, result);
        }

        [Fact]
        public async Task UpdateUserAsync_WhenUnauthorized_ThrowsException()
        {
            var updateDto = new UserUpdateDTO();
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _userService.UpdateUserAsync("1", updateDto, "2"));
        }

        [Fact]
        public async Task UpdateUserAsync_WhenUserNotFound_ThrowsException()
        {
            var updateDto = new UserUpdateDTO();
            _mockUserRepository.Setup(r => r.GetUserByIdAsync("1")).ReturnsAsync((ApplicationUser)null);

            await Assert.ThrowsAsync<ArgumentException>(() => _userService.UpdateUserAsync("1", updateDto, "1"));
        }

        [Fact]
        public async Task UpdateUserAsync_UpdatesAndReturnsUser()
        {
            var existingUser = new ApplicationUser();
            var updateDto = new UserUpdateDTO();
            var userDto = new UserDTO();

            _mockUserRepository.Setup(r => r.GetUserByIdAsync("1")).ReturnsAsync(existingUser);
            _mockMapper.Setup(m => m.Map(updateDto, existingUser));
            _mockUserRepository.Setup(r => r.UpdateUserAsync(existingUser)).Returns(Task.CompletedTask);
            _mockMapper.Setup(m => m.Map<UserDTO>(existingUser)).Returns(userDto);

            var result = await _userService.UpdateUserAsync("1", updateDto, "1");

            Assert.Equal(userDto, result);
        }

        [Fact]
        public async Task DeleteUserAsync_CallsRepository()
        {
            await _userService.DeleteUserAsync("1");
            _mockUserRepository.Verify(r => r.DeleteUserAsync("1"), Times.Once);
        }

        [Fact]
        public async Task GetUserByEmailAsync_ReturnsMappedUser()
        {
            var user = new ApplicationUser();
            var userDto = new UserDTO();

            _mockUserRepository.Setup(r => r.GetUserByEmailAsync("test@example.com")).ReturnsAsync(user);
            _mockMapper.Setup(m => m.Map<UserDTO>(user)).Returns(userDto);

            var result = await _userService.GetUserByEmailAsync("test@example.com");

            Assert.Equal(userDto, result);
        }

    }
}
