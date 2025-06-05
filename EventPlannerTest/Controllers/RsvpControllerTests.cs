using EventPlanner.Controllers;
using EventPlanner.DTOs.RSVP;
using EventPlanner.Enums;
using EventPlanner.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace EventPlannerTest.Controllers
{
    public class RsvpControllerTests
    {
        private readonly Mock<IRsvpService> _mockService;
        private readonly RsvpController _controller;

        public RsvpControllerTests()
        {
            _mockService = new Mock<IRsvpService>();
            _controller = new RsvpController(_mockService.Object);
        }

        [Fact]
        public async Task GetAllRsvps_ReturnsOkWithList()
        {
            var rsvps = new List<RsvpDTO> { new RsvpDTO { Id = 1 }, new RsvpDTO { Id = 2 } };
            _mockService.Setup(s => s.GetAllRsvpsAsync()).ReturnsAsync(rsvps);

            var result = await _controller.GetAllRsvps();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsAssignableFrom<IEnumerable<RsvpDTO>>(okResult.Value);
            Assert.Equal(2, ((List<RsvpDTO>)returnValue).Count);
        }

        [Fact]
        public async Task GetRsvpById_ReturnsOk_WhenFound()
        {
            int rsvpId = 1;
            var rsvp = new RsvpDTO { Id = rsvpId };
            _mockService.Setup(s => s.GetRsvpByIdAsync(rsvpId)).ReturnsAsync(rsvp);

            var result = await _controller.GetRsvpById(rsvpId);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<RsvpDTO>(okResult.Value);
            Assert.Equal(rsvpId, returnValue.Id);
        }

        [Fact]
        public async Task GetRsvpById_ReturnsNotFound_WhenNotFound()
        {
            int rsvpId = 1;
            _mockService.Setup(s => s.GetRsvpByIdAsync(rsvpId)).ThrowsAsync(new ArgumentException("Not found"));

            var result = await _controller.GetRsvpById(rsvpId);

            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Contains("Not found", notFoundResult.Value.ToString());
        }

        [Fact]
        public async Task Create_ReturnsCreatedAtAction_WhenSuccessful()
        {
            var createDto = new RsvpCreateDTO { /* fill with required properties */ };
            var createdDto = new RsvpDTO { Id = 1 };
            _mockService.Setup(s => s.CreateRsvpAsync(createDto)).ReturnsAsync(createdDto);

            var result = await _controller.Create(createDto);

            var createdAtResult = Assert.IsType<CreatedAtActionResult>(result);
            var returnValue = Assert.IsType<RsvpDTO>(createdAtResult.Value);
            Assert.Equal(createdDto.Id, returnValue.Id);
        }

        [Fact]
        public async Task Create_ReturnsBadRequest_WhenModelStateInvalid()
        {
            _controller.ModelState.AddModelError("Key", "Error");

            var createDto = new RsvpCreateDTO();

            var result = await _controller.Create(createDto);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Create_ReturnsBadRequest_WhenArgumentExceptionThrown()
        {
            var createDto = new RsvpCreateDTO();
            _mockService.Setup(s => s.CreateRsvpAsync(createDto)).ThrowsAsync(new ArgumentException("Bad request"));

            var result = await _controller.Create(createDto);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("Bad request", badRequestResult.Value.ToString());
        }

        [Fact]
        public async Task Create_ReturnsConflict_WhenInvalidOperationExceptionThrown()
        {
            var createDto = new RsvpCreateDTO();
            _mockService.Setup(s => s.CreateRsvpAsync(createDto)).ThrowsAsync(new InvalidOperationException("Conflict"));

            var result = await _controller.Create(createDto);

            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            Assert.Contains("Conflict", conflictResult.Value.ToString());
        }

        [Fact]
        public async Task Update_ReturnsOk_WhenSuccessful()
        {
            int rsvpId = 1;
            var updateDto = new RsvpUpdateDTO();
            var updatedDto = new RsvpDTO { Id = rsvpId };

            _mockService.Setup(s => s.UpdateRsvpAsync(rsvpId, updateDto)).ReturnsAsync(updatedDto);

            var result = await _controller.Update(rsvpId, updateDto);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<RsvpDTO>(okResult.Value);
            Assert.Equal(rsvpId, returnValue.Id);
        }

        [Fact]
        public async Task Update_ReturnsNotFound_WhenArgumentExceptionThrown()
        {
            int rsvpId = 1;
            var updateDto = new RsvpUpdateDTO();

            _mockService.Setup(s => s.UpdateRsvpAsync(rsvpId, updateDto)).ThrowsAsync(new ArgumentException("Not found"));

            var result = await _controller.Update(rsvpId, updateDto);

            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Contains("Not found", notFoundResult.Value.ToString());
        }

        [Fact]
        public async Task Update_ReturnsBadRequest_WhenInvalidOperationExceptionThrown()
        {
            int rsvpId = 1;
            var updateDto = new RsvpUpdateDTO();

            _mockService.Setup(s => s.UpdateRsvpAsync(rsvpId, updateDto)).ThrowsAsync(new InvalidOperationException("Bad request"));

            var result = await _controller.Update(rsvpId, updateDto);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("Bad request", badRequestResult.Value.ToString());
        }

        [Fact]
        public async Task Delete_ReturnsNoContent_WhenSuccessful()
        {
            int rsvpId = 1;
            _mockService.Setup(s => s.DeleteRsvpAsync(rsvpId)).Returns(Task.CompletedTask);

            var result = await _controller.Delete(rsvpId);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Delete_ReturnsNotFound_WhenArgumentExceptionThrown()
        {
            int rsvpId = 1;
            _mockService.Setup(s => s.DeleteRsvpAsync(rsvpId)).ThrowsAsync(new ArgumentException("Not found"));

            var result = await _controller.Delete(rsvpId);

            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Contains("Not found", notFoundResult.Value.ToString());
        }

        [Fact]
        public async Task UpdateStatus_ReturnsOk_WhenSuccessful()
        {
            int rsvpId = 1;
            var status = RSVPStatus.Accepted;

            _mockService.Setup(s => s.UpdateStatusAsync(rsvpId, status)).ReturnsAsync(true);

            var result = await _controller.UpdateStatus(rsvpId, status);

            var okResult = Assert.IsType<OkObjectResult>(result);
            // The controller returns an anonymous object with a 'success' property
            using var doc = JsonDocument.Parse(JsonSerializer.Serialize(okResult.Value));
            var root = doc.RootElement;
            Assert.True(root.TryGetProperty("success", out var successProp) && successProp.GetBoolean());
        }

        [Fact]
        public async Task UpdateStatus_ReturnsNotFound_WhenArgumentExceptionThrown()
        {
            int rsvpId = 1;
            var status = RSVPStatus.Accepted;

            _mockService.Setup(s => s.UpdateStatusAsync(rsvpId, status)).ThrowsAsync(new ArgumentException("Not found"));

            var result = await _controller.UpdateStatus(rsvpId, status);

            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Contains("Not found", notFoundResult.Value.ToString());
        }

        [Fact]
        public async Task UpdateStatus_ReturnsBadRequest_WhenExceptionThrown()
        {
            int rsvpId = 1;
            var status = RSVPStatus.Accepted;

            _mockService.Setup(s => s.UpdateStatusAsync(rsvpId, status)).ThrowsAsync(new Exception("Bad request"));

            var result = await _controller.UpdateStatus(rsvpId, status);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("Bad request", badRequestResult.Value.ToString());
        }

        [Fact]
        public async Task GetByEvent_ReturnsOkWithList()
        {
            int eventId = 1;
            var rsvps = new List<RsvpDTO> { new RsvpDTO { Id = 1 }, new RsvpDTO { Id = 2 } };
            _mockService.Setup(s => s.GetRsvpsByEventIdAsync(eventId)).ReturnsAsync(rsvps);

            var result = await _controller.GetByEvent(eventId);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsAssignableFrom<IEnumerable<RsvpDTO>>(okResult.Value);
            Assert.Equal(2, ((List<RsvpDTO>)returnValue).Count);
        }

        [Fact]
        public async Task GetByUser_ReturnsOkWithList()
        {
            int userId = 1;
            var rsvps = new List<RsvpDTO> { new RsvpDTO { Id = 1 }, new RsvpDTO { Id = 2 } };
            _mockService.Setup(s => s.GetRsvpsByUserIdAsync(userId)).ReturnsAsync(rsvps);

            var result = await _controller.GetByUser(userId);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsAssignableFrom<IEnumerable<RsvpDTO>>(okResult.Value);
            Assert.Equal(2, ((List<RsvpDTO>)returnValue).Count);
        }

        [Fact]
        public async Task Cancel_ReturnsOk_WhenSuccessful()
        {
            int rsvpId = 1;
            var cancelledDto = new RsvpDTO { Id = rsvpId };
            _mockService.Setup(s => s.CancelRsvpAsync(rsvpId)).ReturnsAsync(cancelledDto);

            var result = await _controller.Cancel(rsvpId);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<RsvpDTO>(okResult.Value);
            Assert.Equal(rsvpId, returnValue.Id);
        }

        [Fact]
        public async Task Cancel_ReturnsNotFound_WhenArgumentExceptionThrown()
        {
            int rsvpId = 1;
            _mockService.Setup(s => s.CancelRsvpAsync(rsvpId)).ThrowsAsync(new ArgumentException("Not found"));

            var result = await _controller.Cancel(rsvpId);

            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Contains("Not found", notFoundResult.Value.ToString());
        }

        [Fact]
        public async Task RespondToInvite_ReturnsOk_WhenSuccessful()
        {
            int inviteId = 1;
            var status = RSVPStatus.Accepted;
            var responseDto = new RsvpDTO { Id = 1 };
            _mockService.Setup(s => s.RespondToInviteAsync(inviteId, status)).ReturnsAsync(responseDto);

            var result = await _controller.RespondToInvite(inviteId, status);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<RsvpDTO>(okResult.Value);
            Assert.Equal(responseDto.Id, returnValue.Id);
        }

        [Fact]
        public async Task RespondToInvite_ReturnsNotFound_WhenArgumentExceptionThrown()
        {
            int inviteId = 1;
            var status = RSVPStatus.Accepted;
            _mockService.Setup(s => s.RespondToInviteAsync(inviteId, status)).ThrowsAsync(new ArgumentException("Not found"));

            var result = await _controller.RespondToInvite(inviteId, status);

            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Contains("Not found", notFoundResult.Value.ToString());
        }

        [Fact]
        public async Task RespondToInvite_ReturnsConflict_WhenInvalidOperationExceptionThrown()
        {
            int inviteId = 1;
            var status = RSVPStatus.Accepted;
            _mockService.Setup(s => s.RespondToInviteAsync(inviteId, status)).ThrowsAsync(new InvalidOperationException("Conflict"));

            var result = await _controller.RespondToInvite(inviteId, status);

            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            Assert.Contains("Conflict", conflictResult.Value.ToString());
        }

        [Fact]
        public async Task RespondToInvite_ReturnsBadRequest_WhenExceptionThrown()
        {
            int inviteId = 1;
            var status = RSVPStatus.Accepted;
            _mockService.Setup(s => s.RespondToInviteAsync(inviteId, status)).ThrowsAsync(new Exception("Bad request"));

            var result = await _controller.RespondToInvite(inviteId, status);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("Bad request", badRequestResult.Value.ToString());
        }
    }
}
