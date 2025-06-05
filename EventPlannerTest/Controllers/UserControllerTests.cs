using EventPlanner.Controllers;
using EventPlanner.DTOs.User;
using EventPlanner.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

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
                new UserDTO { Id = "1", Username = "user1", FirstName = "First Name", LastName = "Last Name", Role = "User", Email = "user1@example.com", PhoneNumber = "123456" },
                new UserDTO { Id = "2", Username = "user2", FirstName = "First Name", LastName = "Last Name", Role = "Admin", Email = "user2@example.com", PhoneNumber = "456789" }
            };
            _mockService.Setup(s => s.GetAllUsersAsync()).ReturnsAsync(users);

            // Act
            var result = await _controller.GetAllUsers();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsAssignableFrom<IEnumerable<UserDTO>>(okResult.Value);
            Assert.Equal(2, returnValue.Count());
        }

        [Fact]
        public async Task GetUserById_ReturnsOk_WhenUserExists()
        {
            // Arrange
            string userId = "1";
            var user = new UserDTO { Id = userId, Username = "user1", FirstName = "First Name", LastName = "Last Name", Role = "User", Email = "user1@example.com", PhoneNumber = "123456" };

            _mockService.Setup(s => s.GetUserByIdAsync(userId)).ReturnsAsync(user);

            // Act
            var result = await _controller.GetUserById(userId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<UserDTO>(okResult.Value);
            Assert.Equal(userId, returnValue.Id);
        }

        [Fact]
        public async Task GetUserById_ReturnsNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            string userId = "1";

            _mockService.Setup(s => s.GetUserByIdAsync(userId)).ReturnsAsync((UserDTO)null);

            // Act
            var result = await _controller.GetUserById(userId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Equal("User not found.", notFoundResult.Value);
        }

        [Fact]
        public async Task CreateUser_ReturnsCreatedAtAction_WithCreatedUser()
        {
            // Arrange
            var newUser = new UserCreateDTO { Username = "newuser", FirstName = "New", LastName = "User", Email = "new@email.com", PhoneNumber = "1234567890" };
            var createdUser = new UserDTO { Id = "1", Username = "newuser", FirstName = "New", LastName = "User", Email = "new@email.com", PhoneNumber = "1234567890" };

            _mockService.Setup(s => s.CreateUserAsync(newUser)).ReturnsAsync(createdUser);

            // Act
            var result = await _controller.CreateUser(newUser);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var returnValue = Assert.IsType<UserDTO>(createdAtActionResult.Value);
            Assert.Equal(createdUser.Id, returnValue.Id);
        }

        [Fact]
        public async Task UpdateUser_ReturnsNoContent_WhenUpdateIsSuccessful()
        {
            // Arrange
            string userId = "1";
            var userUpdateDto = new UserUpdateDTO
            {
                FirstName = "Updated",
                LastName = "User",
                PhoneNumber = "0987654321"
            };

            _mockService.Setup(s => s.UpdateUserAsync(userId, userUpdateDto, It.IsAny<string>()))
                .ReturnsAsync(new UserDTO
                {
                    Id = userId,
                    FirstName = "Updated",
                    LastName = "User",
                    PhoneNumber = "0987654321"
                });

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim("id", userId)
            }, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            // Act
            var result = await _controller.UpdateUser(userId, userUpdateDto);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteUser_ReturnsNoContent_WhenDeleted()
        {
            // Arrange
            string userId = "1";
            _mockService.Setup(s => s.DeleteUserAsync(userId)).Returns(Task.CompletedTask);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Role, "Admin")
            }, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            // Act
            var result = await _controller.DeleteUser(userId);

            // Assert
            Assert.IsType<NoContentResult>(result);
            _mockService.Verify(s => s.DeleteUserAsync(userId), Times.Once);
        }


        [Fact]
        public async Task GetUserByEmail_ReturnsOk_WhenUserExists()
        {
            // Arrange
            string email = "test@example.com";
            var userDto = new UserDTO { Id = "1", Email = email, Username = "user1" };
            _mockService.Setup(s => s.GetUserByEmailAsync(email)).ReturnsAsync(userDto);

            // Act
            var result = await _controller.GetUserByEmail(email);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<UserDTO>(okResult.Value);
            Assert.Equal(email, returnValue.Email);
        }

        [Fact]
        public async Task GetUserByEmail_ReturnsNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            string email = "missing@example.com";
            _mockService.Setup(s => s.GetUserByEmailAsync(email)).ReturnsAsync((UserDTO)null);

            // Act
            var result = await _controller.GetUserByEmail(email);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Equal("User not found.", notFoundResult.Value);
        }

        [Fact]
        public async Task CreateUser_ReturnsBadRequest_WhenUserIsNull()
        {
            // Act
            var result = await _controller.CreateUser(null);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("User cannot be null.", badRequestResult.Value);
        }

        [Fact]
        public async Task UpdateUser_ReturnsForbid_WhenUnauthorizedAccessExceptionThrown()
        {
            // Arrange
            string userId = "1";
            var userUpdateDto = new UserUpdateDTO();

            _mockService.Setup(s => s.UpdateUserAsync(userId, userUpdateDto, It.IsAny<string>()))
                .ThrowsAsync(new UnauthorizedAccessException("Unauthorized"));

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim("id", userId)
            }, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            // Act
            var result = await _controller.UpdateUser(userId, userUpdateDto);

            // Assert
            var forbidResult = Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task UpdateUser_ReturnsNotFound_WhenArgumentExceptionThrown()
        {
            // Arrange
            string userId = "1";
            var userUpdateDto = new UserUpdateDTO();

            _mockService.Setup(s => s.UpdateUserAsync(userId, userUpdateDto, It.IsAny<string>()))
                .ThrowsAsync(new ArgumentException("User not found"));

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim("id", userId)
            }, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            // Act
            var result = await _controller.UpdateUser(userId, userUpdateDto);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("User not found", notFoundResult.Value);
        }

        [Fact]
        public async Task UpdateUser_ReturnsStatusCode500_WhenGenericExceptionThrown()
        {
            // Arrange
            string userId = "1";
            var userUpdateDto = new UserUpdateDTO();

            _mockService.Setup(s => s.UpdateUserAsync(userId, userUpdateDto, It.IsAny<string>()))
                .ThrowsAsync(new Exception("Some error"));

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim("id", userId)
            }, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            // Act
            var result = await _controller.UpdateUser(userId, userUpdateDto);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            Assert.Equal("Some error", statusCodeResult.Value);
        }
    }
}
