using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Identity;
using EventPlanner.Controllers;
using EventPlanner.Models;
using EventPlanner.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace EventPlanner.Tests
{
    public class AccountControllerTests
    {
        private Mock<UserManager<ApplicationUser>> MockUserManager()
        {
            var store = new Mock<IUserStore<ApplicationUser>>();
            return new Mock<UserManager<ApplicationUser>>(store.Object, null, null, null, null, null, null, null, null);
        }

        private Mock<RoleManager<IdentityRole>> MockRoleManager()
        {
            var store = new Mock<IRoleStore<IdentityRole>>();
            return new Mock<RoleManager<IdentityRole>>(store.Object, null, null, null, null);
        }

        [Fact]
        public async Task Register_ValidUser_ReturnsOk()
        {
            var mockUserManager = MockUserManager();
            var mockRoleManager = MockRoleManager();
            var mockConfig = new Mock<IConfiguration>();
            var controller = new AccountController(mockUserManager.Object, mockConfig.Object, mockRoleManager.Object);

            var model = new RegisterModelDTO
            {
                Username = "testuser",
                Email = "test@example.com",
                Password = "Test123!",
                FirstName = "Test",
                LastName = "User",
                PhoneNumber = "1234567890"
            };

            mockUserManager.Setup(m => m.FindByEmailAsync(model.Email)).ReturnsAsync((ApplicationUser)null);
            mockUserManager.Setup(m => m.FindByNameAsync(model.Username)).ReturnsAsync((ApplicationUser)null);
            mockUserManager.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), model.Password))
                           .ReturnsAsync(IdentityResult.Success);
            mockUserManager.Setup(m => m.AddToRoleAsync(It.IsAny<ApplicationUser>(), "User"))
                           .ReturnsAsync(IdentityResult.Success);

            var result = await controller.Register(model);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task Login_ValidUser_ReturnsToken()
        {
            var mockUserManager = MockUserManager();
            var mockConfig = new Mock<IConfiguration>();
            var mockRoleManager = MockRoleManager();

            var user = new ApplicationUser { UserName = "testuser", Id = "123" };
            var controller = new AccountController(mockUserManager.Object, mockConfig.Object, mockRoleManager.Object);

            var loginModel = new LoginModelDTO { EmailOrUsername = "testuser", Password = "Test123!" };

            mockUserManager.Setup(m => m.FindByNameAsync(loginModel.EmailOrUsername)).ReturnsAsync(user);
            mockUserManager.Setup(m => m.CheckPasswordAsync(user, loginModel.Password)).ReturnsAsync(true);
            mockUserManager.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(new List<string> { "User" });

            mockConfig.Setup(c => c["Jwt:Key"]).Returns("ThisIsASecretKeyForTestingPurposesOnly");
            mockConfig.Setup(c => c["Jwt:Issuer"]).Returns("testIssuer");
            mockConfig.Setup(c => c["Jwt:Audience"]).Returns("testAudience");

            var result = await controller.Login(loginModel);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task AssignRoleToUser_Valid_ReturnsOk()
        {
            var mockUserManager = MockUserManager();
            var mockRoleManager = MockRoleManager();
            var mockConfig = new Mock<IConfiguration>();

            var user = new ApplicationUser { Id = "123", UserName = "testuser" };
            var controller = new AccountController(mockUserManager.Object, mockConfig.Object, mockRoleManager.Object);

            var dto = new UserRoleAssignmentDTO { UserId = "123", RoleName = "Admin" };

            mockUserManager.Setup(m => m.FindByIdAsync(dto.UserId)).ReturnsAsync(user);
            mockRoleManager.Setup(r => r.RoleExistsAsync(dto.RoleName)).ReturnsAsync(true);
            mockUserManager.Setup(m => m.AddToRoleAsync(user, dto.RoleName)).ReturnsAsync(IdentityResult.Success);

            // Mock the User property to provide a ClaimsPrincipal with NameIdentifier and Admin role
            var claims = new List<System.Security.Claims.Claim>
            {
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "999"),
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, "Admin")
            };
            var identity = new System.Security.Claims.ClaimsIdentity(claims, "TestAuthType");
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext
                {
                    User = new System.Security.Claims.ClaimsPrincipal(identity)
                }
            };

            mockUserManager.Setup(m => m.FindByIdAsync("999")).ReturnsAsync(new ApplicationUser { Id = "999", UserName = "adminuser" });
            mockUserManager.Setup(m => m.IsInRoleAsync(It.Is<ApplicationUser>(u => u.Id == "999"), "Admin")).ReturnsAsync(true);

            var result = await controller.AssignRoleToUser(dto);

            Assert.IsType<OkObjectResult>(result);
        }
    }
}
