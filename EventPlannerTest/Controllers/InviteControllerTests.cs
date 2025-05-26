using EventPlanner.Controllers;
using EventPlanner.DTOs.Invite;
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
    public class InviteControllerTests
    {
        private readonly Mock<IInviteService> _mockInviteService;
        private readonly InviteController _controller;

        public InviteControllerTests()
        {
            _mockInviteService = new Mock<IInviteService>();
            _controller = new InviteController(_mockInviteService.Object);
        }

        [Fact]
        public async Task GetAllInvites_ReturnsOkResultWithList()
        {
            var invites = new List<InviteDTO> { new InviteDTO { Id = 1 } };
            _mockInviteService.Setup(s => s.GetAllInvitesAsync()).ReturnsAsync(invites);

            var result = await _controller.GetAllInvites();

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(invites, okResult.Value);
        }

        [Fact]
        public async Task GetInviteById_ValidId_ReturnsInvite()
        {
            var invite = new InviteDTO { Id = 1 };
            _mockInviteService.Setup(s => s.GetInviteByIdAsync(1)).ReturnsAsync(invite);

            var result = await _controller.GetInviteById(1);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(invite, okResult.Value);
        }

        [Fact]
        public async Task GetInviteById_InvalidId_ReturnsNotFound()
        {
            _mockInviteService.Setup(s => s.GetInviteByIdAsync(999)).ReturnsAsync((InviteDTO)null);

            var result = await _controller.GetInviteById(999);

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task SendInvite_ValidInvite_ReturnsCreatedAtAction()
        {
            var inviteCreate = new InviteCreateDTO { InviteeId = 2, InviterId = 1, EventId = 5 };
            var createdInvite = new InviteDTO { Id = 10 };
            _mockInviteService.Setup(s => s.SendInviteAsync(inviteCreate)).ReturnsAsync(createdInvite);

            var result = await _controller.SendInvite(inviteCreate);

            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(createdInvite, createdResult.Value);
        }

        [Fact]
        public async Task SendInvite_InvalidInvite_ReturnsBadRequest()
        {
            var inviteCreate = new InviteCreateDTO();
            _mockInviteService.Setup(s => s.SendInviteAsync(inviteCreate)).ThrowsAsync(new Exception("Error"));

            var result = await _controller.SendInvite(inviteCreate);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Error", badRequest.Value);
        }

        [Fact]
        public async Task UpdateInvite_Valid_ReturnsOk()
        {
            var updateDto = new InviteUpdateDTO { Status = EventPlanner.Enums.InviteStatus.Accepted };
            var updated = new InviteDTO { Id = 1, Status = EventPlanner.Enums.InviteStatus.Accepted };
            _mockInviteService.Setup(s => s.UpdateInviteAsync(1, updateDto)).ReturnsAsync(updated);

            var result = await _controller.UpdateInvite(1, updateDto);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(updated, okResult.Value);
        }

        [Fact]
        public async Task UpdateInvite_Exception_ReturnsBadRequest()
        {
            var updateDto = new InviteUpdateDTO { Status = EventPlanner.Enums.InviteStatus.Accepted };
            _mockInviteService.Setup(s => s.UpdateInviteAsync(1, updateDto)).ThrowsAsync(new Exception("Update failed"));

            var result = await _controller.UpdateInvite(1, updateDto);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Update failed", badRequest.Value);
        }

        [Fact]
        public async Task DeleteInvite_Valid_ReturnsNoContent()
        {
            _mockInviteService.Setup(s => s.DeleteInviteAsync(1)).Returns(Task.CompletedTask);

            var result = await _controller.DeleteInvite(1);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteInvite_Exception_ReturnsBadRequest()
        {
            _mockInviteService.Setup(s => s.DeleteInviteAsync(1)).ThrowsAsync(new Exception("Delete failed"));

            var result = await _controller.DeleteInvite(1);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Delete failed", badRequest.Value);
        }

        [Fact]
        public async Task GetPendingInvitesByUserId_ReturnsPendingInvites()
        {
            var invites = new List<InviteDTO> { new InviteDTO { Id = 1 } };
            _mockInviteService.Setup(s => s.GetPendingInvitesByUserIdAsync(2)).ReturnsAsync(invites);

            var result = await _controller.GetPendingInvitesByUserId(2);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(invites, okResult.Value);
        }

    }
}
