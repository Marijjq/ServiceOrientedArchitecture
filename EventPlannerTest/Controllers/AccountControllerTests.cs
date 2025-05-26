using EventPlanner.Controllers;
using EventPlanner.DTOs;
using EventPlanner.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace EventPlannerTest.Controllers
{
    public class AccountControllerTests
    {
        private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
        private readonly Mock<RoleManager<IdentityRole>> _mockRoleManager;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly AccountController _controller;

        public AccountControllerTests()
        {
            _mockUserManager = MockUserManager<ApplicationUser>();
            _mockRoleManager = MockRoleManager<IdentityRole>();
            _mockConfiguration = new Mock<IConfiguration>();

            _mockConfiguration.Setup(c => c["Jwt:Key"]).Returns("ThisIsASecretKeyForJwtTokenGeneration");
            _mockConfiguration.Setup(c => c["Jwt:Issuer"]).Returns("TestIssuer");
            _mockConfiguration.Setup(c => c["Jwt:Audience"]).Returns("TestAudience");

            _controller = new AccountController(_mockUserManager.Object, _mockConfiguration.Object, _mockRoleManager.Object);
        }

        #region Register Tests

        [Fact]
        public async Task Register_ValidUser_ReturnsOk()
        {
            var model = new RegisterModelDTO
            {
                Username = "newuser",
                Email = "newuser@example.com",
                Password = "Password123!",
                FirstName = "New",
                LastName = "User",
                PhoneNumber = "1234567890"
            };

            _mockUserManager.Setup(um => um.FindByEmailAsync(model.Email)).ReturnsAsync((ApplicationUser)null);
            _mockUserManager.Setup(um => um.FindByNameAsync(model.Username)).ReturnsAsync((ApplicationUser)null);
            _mockUserManager.Setup(um => um.CreateAsync(It.IsAny<ApplicationUser>(), model.Password)).ReturnsAsync(IdentityResult.Success);
            _mockUserManager.Setup(um => um.AddToRoleAsync(It.IsAny<ApplicationUser>(), "User")).ReturnsAsync(IdentityResult.Success);

            var result = await _controller.Register(model);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Contains("User registered successfully", okResult.Value.ToString());
        }

        [Fact]
        public async Task Register_EmailAlreadyExists_ReturnsConflict()
        {
            var model = new RegisterModelDTO
            {
                Username = "newuser",
                Email = "existing@example.com",
                Password = "Password123!",
            };

            _mockUserManager.Setup(um => um.FindByEmailAsync(model.Email))
                            .ReturnsAsync(new ApplicationUser());

            var result = await _controller.Register(model);

            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            Assert.Contains("email already exists", conflictResult.Value.ToString().ToLower());
        }

        [Fact]
        public async Task Register_UsernameAlreadyExists_ReturnsConflict()
        {
            var model = new RegisterModelDTO
            {
                Username = "existinguser",
                Email = "newemail@example.com",
                Password = "Password123!",
            };

            _mockUserManager.Setup(um => um.FindByEmailAsync(model.Email)).ReturnsAsync((ApplicationUser)null);
            _mockUserManager.Setup(um => um.FindByNameAsync(model.Username)).ReturnsAsync(new ApplicationUser());

            var result = await _controller.Register(model);

            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            Assert.Contains("username already exists", conflictResult.Value.ToString().ToLower());
        }

        [Fact]
        public async Task Register_CreateFails_ReturnsServerError()
        {
            var model = new RegisterModelDTO
            {
                Username = "newuser",
                Email = "newuser@example.com",
                Password = "Password123!",
            };

            _mockUserManager.Setup(um => um.FindByEmailAsync(model.Email)).ReturnsAsync((ApplicationUser)null);
            _mockUserManager.Setup(um => um.FindByNameAsync(model.Username)).ReturnsAsync((ApplicationUser)null);

            var failedResult = IdentityResult.Failed(new IdentityError { Description = "Password too weak" });
            _mockUserManager.Setup(um => um.CreateAsync(It.IsAny<ApplicationUser>(), model.Password)).ReturnsAsync(failedResult);

            var result = await _controller.Register(model);

            var errorResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, errorResult.StatusCode);
            Assert.Contains("User creation failed", errorResult.Value.ToString());
        }

        #endregion

        #region Login Tests

        [Fact]
        public async Task Login_Success_ReturnsOkWithToken()
        {
            var loginDto = new LoginModelDTO
            {
                EmailOrUsername = "testuser",
                Password = "Password123!"
            };

            var user = new ApplicationUser
            {
                UserName = "testuser",
                Email = "testuser@example.com"
            };

            // UserManager setups
            _mockUserManager.Setup(um => um.FindByEmailAsync(loginDto.EmailOrUsername))
                            .ReturnsAsync((ApplicationUser)null);
            _mockUserManager.Setup(um => um.FindByNameAsync(loginDto.EmailOrUsername))
                            .ReturnsAsync(user);
            _mockUserManager.Setup(um => um.CheckPasswordAsync(user, loginDto.Password))
                            .ReturnsAsync(true);
            _mockUserManager.Setup(um => um.GetRolesAsync(user))
                            .ReturnsAsync(new List<string> { "User" });

            var result = await _controller.Login(loginDto);

            var okResult = Assert.IsType<OkObjectResult>(result);

            var value = okResult.Value;
            var dictionary = value.GetType()
                                  .GetProperties()
                                  .ToDictionary(prop => prop.Name, prop => prop.GetValue(value, null));

            Assert.True(dictionary.ContainsKey("token"));
            Assert.False(string.IsNullOrEmpty(dictionary["token"]?.ToString()));

            Assert.True(dictionary.ContainsKey("expiration"));
            Assert.NotNull(dictionary["expiration"]);
        }

        [Fact]
        public async Task Login_InvalidUser_ReturnsUnauthorized()
        {
            var loginDto = new LoginModelDTO
            {
                EmailOrUsername = "nonexistent",
                Password = "Password123!"
            };

            _mockUserManager.Setup(um => um.FindByEmailAsync(loginDto.EmailOrUsername))
                            .ReturnsAsync((ApplicationUser)null);
            _mockUserManager.Setup(um => um.FindByNameAsync(loginDto.EmailOrUsername))
                            .ReturnsAsync((ApplicationUser)null);

            var result = await _controller.Login(loginDto);

            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Contains("Invalid login attempt", unauthorizedResult.Value.ToString());
        }

        [Fact]
        public async Task Login_WrongPassword_ReturnsUnauthorized()
        {
            var loginDto = new LoginModelDTO
            {
                EmailOrUsername = "testuser",
                Password = "WrongPassword!"
            };

            var user = new ApplicationUser { UserName = "testuser" };

            _mockUserManager.Setup(um => um.FindByEmailAsync(loginDto.EmailOrUsername))
                            .ReturnsAsync((ApplicationUser)null);
            _mockUserManager.Setup(um => um.FindByNameAsync(loginDto.EmailOrUsername))
                            .ReturnsAsync(user);
            _mockUserManager.Setup(um => um.CheckPasswordAsync(user, loginDto.Password))
                            .ReturnsAsync(false);

            var result = await _controller.Login(loginDto);

            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Contains("Invalid login attempt", unauthorizedResult.Value.ToString());
        }

        #endregion

        #region Role Management Tests

        [Fact]
        public async Task CreateRole_ValidRole_ReturnsOk()
        {
            string roleName = "Admin";

            _mockRoleManager.Setup(rm => rm.RoleExistsAsync(roleName)).ReturnsAsync(false);
            _mockRoleManager.Setup(rm => rm.CreateAsync(It.IsAny<IdentityRole>())).ReturnsAsync(IdentityResult.Success);

            var result = await _controller.CreateRole(roleName);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Contains("created successfully", okResult.Value.ToString());
        }

        [Fact]
        public async Task CreateRole_ExistingRole_ReturnsConflict()
        {
            string roleName = "Admin";

            _mockRoleManager.Setup(rm => rm.RoleExistsAsync(roleName)).ReturnsAsync(true);

            var result = await _controller.CreateRole(roleName);

            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            Assert.Contains("already exists", conflictResult.Value.ToString());
        }

        [Fact]
        public async Task CreateRole_Failure_ReturnsServerError()
        {
            string roleName = "Admin";

            _mockRoleManager.Setup(rm => rm.RoleExistsAsync(roleName)).ReturnsAsync(false);
            var failedResult = IdentityResult.Failed(new IdentityError { Description = "Some error" });
            _mockRoleManager.Setup(rm => rm.CreateAsync(It.IsAny<IdentityRole>())).ReturnsAsync(failedResult);

            var result = await _controller.CreateRole(roleName);

            var errorResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, errorResult.StatusCode);
            Assert.Contains("Role creation failed", errorResult.Value.ToString());
        }

        [Fact]
        public async Task AssignRoleToUser_Valid_ReturnsOk()
        {
            var dto = new UserRoleAssignmentDTO
            {
                UserId = "123",
                RoleName = "User"
            };

            var user = new ApplicationUser { UserName = "testuser" };

            _mockUserManager.Setup(um => um.FindByIdAsync(dto.UserId)).ReturnsAsync(user);
            _mockRoleManager.Setup(rm => rm.RoleExistsAsync(dto.RoleName)).ReturnsAsync(true);
            _mockUserManager.Setup(um => um.AddToRoleAsync(user, dto.RoleName)).ReturnsAsync(IdentityResult.Success);

            var result = await _controller.AssignRoleToUser(dto);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Contains("assigned to user", okResult.Value.ToString());
        }

        [Fact]
        public async Task AssignRoleToUser_MissingUserIdOrRoleName_ReturnsBadRequest()
        {
            var dto = new UserRoleAssignmentDTO
            {
                UserId = "",
                RoleName = null
            };

            var result = await _controller.AssignRoleToUser(dto);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("required", badRequest.Value.ToString());
        }

        [Fact]
        public async Task AssignRoleToUser_UserNotFound_ReturnsNotFound()
        {
            var dto = new UserRoleAssignmentDTO
            {
                UserId = "notfound",
                RoleName = "User"
            };

            _mockUserManager.Setup(um => um.FindByIdAsync(dto.UserId)).ReturnsAsync((ApplicationUser)null);

            var result = await _controller.AssignRoleToUser(dto);

            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Contains("User not found", notFound.Value.ToString());
        }

        [Fact]
        public async Task AssignRoleToUser_RoleNotFound_ReturnsNotFound()
        {
            var dto = new UserRoleAssignmentDTO
            {
                UserId = "123",
                RoleName = "NonExistingRole"
            };

            var user = new ApplicationUser();

            _mockUserManager.Setup(um => um.FindByIdAsync(dto.UserId)).ReturnsAsync(user);
            _mockRoleManager.Setup(rm => rm.RoleExistsAsync(dto.RoleName)).ReturnsAsync(false);

            var result = await _controller.AssignRoleToUser(dto);

            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Contains("Role does not exist", notFound.Value.ToString());
        }

        [Fact]
        public async Task AssignRoleToUser_Failure_ReturnsServerError()
        {
            var dto = new UserRoleAssignmentDTO
            {
                UserId = "123",
                RoleName = "User"
            };

            var user = new ApplicationUser();

            _mockUserManager.Setup(um => um.FindByIdAsync(dto.UserId)).ReturnsAsync(user);
            _mockRoleManager.Setup(rm => rm.RoleExistsAsync(dto.RoleName)).ReturnsAsync(true);

            var failedResult = IdentityResult.Failed(new IdentityError { Description = "Failed to add role" });
            _mockUserManager.Setup(um => um.AddToRoleAsync(user, dto.RoleName)).ReturnsAsync(failedResult);

            var result = await _controller.AssignRoleToUser(dto);

            var errorResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, errorResult.StatusCode);
            Assert.Contains("Failed to assign", errorResult.Value.ToString());
        }

        #endregion

        // Helper method to mock UserManager
        private static Mock<UserManager<TUser>> MockUserManager<TUser>() where TUser : class
        {
            var store = new Mock<IUserStore<TUser>>();
            return new Mock<UserManager<TUser>>(store.Object, null, null, null, null, null, null, null, null);
        }

        // Helper method to mock RoleManager
        private static Mock<RoleManager<TRole>> MockRoleManager<TRole>() where TRole : class
        {
            var store = new Mock<IRoleStore<TRole>>();
            return new Mock<RoleManager<TRole>>(store.Object, null, null, null, null);
        }
    }
}
