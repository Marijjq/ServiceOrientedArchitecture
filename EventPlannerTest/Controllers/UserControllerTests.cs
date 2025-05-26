using EventPlanner.Controllers;
using EventPlanner.DTOs.User;
using EventPlanner.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventPlannerTest.Controllers
{
    public class UserControllerTests
    {
        private readonly Mock<IUserService> _mockService;
        private readonly UserController _controller;

        public UserControllerTests()
        {
            _mockService = new Mock<IUserService>();
            _controller = new UserController(_mockService.Object);
        }

        [Fact]
        public async Task GetAllUsers_ReturnsOkResult_WithListOfUsers()
        {
            // Arrange
            var users = new List<UserDTO>
            {
                new UserDTO { Id = 1, Username = "User1", Email = "user1@example.com" },
                new UserDTO { Id = 2, Username = "User2", Email = "user2@example.com" }
            };

            _mockService.Setup(s => s.GetAllUsersAsync()).ReturnsAsync(users);

            // Act
            var result = await _controller.GetAllUsers();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnUsers = Assert.IsAssignableFrom<IEnumerable<UserDTO>>(okResult.Value);
            Assert.Equal(2, ((List<UserDTO>)returnUsers).Count);
        }

        [Fact]
        public async Task GetUserById_ExistingId_ReturnsOkResult()
        {
            // Arrange
            var user = new UserDTO { Id = 1, Username = "User1", Email = "user1@example.com" };
            _mockService.Setup(s => s.GetUserByIdAsync(1)).ReturnsAsync(user);

            // Act
            var result = await _controller.GetUserById(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedUser = Assert.IsType<UserDTO>(okResult.Value);
            Assert.Equal("User1", returnedUser.Username);
        }

        [Fact]
        public async Task GetUserById_NonExistingId_ReturnsNotFound()
        {
            _mockService.Setup(s => s.GetUserByIdAsync(99)).ReturnsAsync((UserDTO)null);

            var result = await _controller.GetUserById(99);

            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Equal("User not found.", notFoundResult.Value);
        }

        [Fact]
        public async Task CreateUser_ValidUser_ReturnsCreatedAtAction()
        {
            var userCreate = new UserCreateDTO { Username = "NewUser", Email = "new@example.com", Password = "123456" };
            var createdUser = new UserDTO { Id = 3, Username = "NewUser", Email = "new@example.com" };

            _mockService.Setup(s => s.CreateUserAsync(userCreate)).ReturnsAsync(createdUser);

            var result = await _controller.CreateUser(userCreate);

            var createdAt = Assert.IsType<CreatedAtActionResult>(result.Result);
            var returnedUser = Assert.IsType<UserDTO>(createdAt.Value);
            Assert.Equal("NewUser", returnedUser.Username);
        }

        [Fact]
        public async Task UpdateUser_ExistingUser_ReturnsNoContent()
        {
            var updateDto = new UserUpdateDTO { FirstName = "Updated", LastName = "User" };
            _mockService.Setup(s => s.UpdateUserAsync(1, updateDto)).ReturnsAsync(new UserDTO());

            var result = await _controller.UpdateUser(1, updateDto);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task UpdateUser_NonExistingUser_ReturnsNotFound()
        {
            var updateDto = new UserUpdateDTO { FirstName = "Test", LastName = "User" };
            _mockService.Setup(s => s.UpdateUserAsync(99, updateDto)).ReturnsAsync((UserDTO)null);

            var result = await _controller.UpdateUser(99, updateDto);

            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("User not found.", notFound.Value);
        }

        [Fact]
        public async Task DeleteUser_ValidId_ReturnsNoContent()
        {
            var result = await _controller.DeleteUser(1);

            Assert.IsType<NoContentResult>(result);
            _mockService.Verify(s => s.DeleteUserAsync(1), Times.Once);
        }

        [Fact]
        public async Task GetUserByEmail_ExistingEmail_ReturnsOk()
        {
            var user = new UserDTO { Id = 1, Username = "User1", Email = "user1@example.com" };
            _mockService.Setup(s => s.GetUserByEmailAsync("user1@example.com")).ReturnsAsync(user);

            var result = await _controller.GetUserByEmail("user1@example.com");

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedUser = Assert.IsType<UserDTO>(okResult.Value);
            Assert.Equal("user1@example.com", returnedUser.Email);
        }

        [Fact]
        public async Task GetUserByEmail_NonExistingEmail_ReturnsNotFound()
        {
            _mockService.Setup(s => s.GetUserByEmailAsync("nonexist@example.com")).ReturnsAsync((UserDTO)null);

            var result = await _controller.GetUserByEmail("nonexist@example.com");

            var notFound = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Equal("User not found.", notFound.Value);
        }
    }
}
