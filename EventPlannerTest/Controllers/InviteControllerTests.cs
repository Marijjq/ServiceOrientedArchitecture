using EventPlanner.Controllers;
using EventPlanner.DTOs.Invite;
using EventPlanner.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace EventPlannerTest.Controllers
{
    public class InviteControllerTests
    {
        private readonly Mock<IInviteService> _mockService;
        private readonly InviteController _controller;

        public InviteControllerTests()
        {
            _mockService = new Mock<IInviteService>();
            _controller = new InviteController(_mockService.Object);
        }

        [Fact]
        public async Task GetAllInvites_ReturnsOk_WithInvites()
        {
            var invites = new List<InviteDTO> { new InviteDTO { Id = 1 }, new InviteDTO { Id = 2 } };
            _mockService.Setup(s => s.GetAllInvitesAsync()).ReturnsAsync(invites);

            var result = await _controller.GetAllInvites();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedInvites = Assert.IsAssignableFrom<IEnumerable<InviteDTO>>(okResult.Value);
            Assert.Equal(2, ((List<InviteDTO>)returnedInvites).Count);
        }

        [Fact]
        public async Task GetInviteById_ReturnsOk_WhenInviteExists()
        {
            var invite = new InviteDTO { Id = 5 };
            _mockService.Setup(s => s.GetInviteByIdAsync(5)).ReturnsAsync(invite);

            var result = await _controller.GetInviteById(5);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedInvite = Assert.IsType<InviteDTO>(okResult.Value);
            Assert.Equal(5, returnedInvite.Id);
        }

        [Fact]
        public async Task GetInviteById_ReturnsNotFound_WhenInviteDoesNotExist()
        {
            _mockService.Setup(s => s.GetInviteByIdAsync(10)).ReturnsAsync((InviteDTO)null);

            var result = await _controller.GetInviteById(10);

            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Invite not found", notFoundResult.Value);
        }

        [Fact]
        public async Task SendInvite_ReturnsCreatedAtAction_WhenSuccessful()
        {
            var createDto = new InviteCreateDTO();
            var createdInvite = new InviteDTO { Id = 1 };

            _mockService.Setup(s => s.SendInviteAsync(createDto)).ReturnsAsync(createdInvite);

            var result = await _controller.SendInvite(createDto);

            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(_controller.GetInviteById), createdResult.ActionName);
            var returnedInvite = Assert.IsType<InviteDTO>(createdResult.Value);
            Assert.Equal(1, returnedInvite.Id);
        }

        [Fact]
        public async Task SendInvite_ReturnsBadRequest_WhenExceptionThrown()
        {
            var createDto = new InviteCreateDTO();
            _mockService.Setup(s => s.SendInviteAsync(createDto)).ThrowsAsync(new Exception("Error"));

            var result = await _controller.SendInvite(createDto);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Error", badRequest.Value);
        }

        [Fact]
        public async Task UpdateInvite_ReturnsOk_WhenSuccessful()
        {
            var updateDto = new InviteUpdateDTO();
            var updatedInvite = new InviteDTO { Id = 2 };

            _mockService.Setup(s => s.UpdateInviteAsync(2, updateDto)).ReturnsAsync(updatedInvite);

            var result = await _controller.UpdateInvite(2, updateDto);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedInvite = Assert.IsType<InviteDTO>(okResult.Value);
            Assert.Equal(2, returnedInvite.Id);
        }

        [Fact]
        public async Task UpdateInvite_ReturnsBadRequest_WhenExceptionThrown()
        {
            var updateDto = new InviteUpdateDTO();
            _mockService.Setup(s => s.UpdateInviteAsync(3, updateDto)).ThrowsAsync(new Exception("Error"));

            var result = await _controller.UpdateInvite(3, updateDto);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Error", badRequest.Value);
        }

        [Fact]
        public async Task DeleteInvite_ReturnsNoContent_WhenSuccessful()
        {
            _mockService.Setup(s => s.DeleteInviteAsync(4)).Returns(Task.CompletedTask);

            var result = await _controller.DeleteInvite(4);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteInvite_ReturnsBadRequest_WhenExceptionThrown()
        {
            _mockService.Setup(s => s.DeleteInviteAsync(5)).ThrowsAsync(new Exception("Error"));

            var result = await _controller.DeleteInvite(5);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Error", badRequest.Value);
        }

        [Fact]
        public async Task GetPendingInvitesByUserId_ReturnsOk_WithInvites()
        {
            // Arrange: Set up a fake user with required claims and roles
            var userId = "7";
            var claims = new List<Claim>
            {
                new Claim("id", userId.ToString()),
                new Claim(ClaimTypes.Role, "Admin") // or "Organizer" if you want
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            var invites = new List<InviteDTO> { new InviteDTO { Id = 1 } };
            _mockService.Setup(s => s.GetPendingInvitesByUserIdAsync(userId)).ReturnsAsync(invites);

            // Act
            var result = await _controller.GetPendingInvitesByUserId(userId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedInvites = Assert.IsAssignableFrom<IEnumerable<InviteDTO>>(okResult.Value);
            Assert.Single(returnedInvites);
        }
    }
}
