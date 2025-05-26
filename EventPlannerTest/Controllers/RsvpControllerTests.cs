using EventPlanner.Controllers;
using EventPlanner.DTOs.RSVP;
using EventPlanner.Enums;
using EventPlanner.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using System.Text.Json;

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
        public async Task GetAllRsvps_ReturnsOk()
        {
            var rsvps = new List<RsvpDTO> { new RsvpDTO { Id = 1 } };
            _mockService.Setup(s => s.GetAllRsvpsAsync()).ReturnsAsync(rsvps);

            var result = await _controller.GetAllRsvps();
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(rsvps, ok.Value);
        }

        [Fact]
        public async Task GetRsvpById_ValidId_ReturnsOk()
        {
            var rsvp = new RsvpDTO { Id = 1 };
            _mockService.Setup(s => s.GetRsvpByIdAsync(1)).ReturnsAsync(rsvp);

            var result = await _controller.GetRsvpById(1);
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(rsvp, ok.Value);
        }

        [Fact]
        public async Task GetRsvpById_InvalidId_ReturnsNotFound()
        {
            _mockService.Setup(s => s.GetRsvpByIdAsync(999)).ThrowsAsync(new ArgumentException("Not found"));

            var result = await _controller.GetRsvpById(999);
            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Contains("Not found", notFound.Value.ToString());
        }

        [Fact]
        public async Task Create_Valid_ReturnsCreated()
        {
            var dto = new RsvpCreateDTO { UserId = 1, EventId = 2, Status = RSVPStatus.Accepted };
            var created = new RsvpDTO { Id = 1 };
            _mockService.Setup(s => s.CreateRsvpAsync(dto)).ReturnsAsync(created);

            var result = await _controller.Create(dto);
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(created, createdResult.Value);
        }

        [Fact]
        public async Task Update_Valid_ReturnsOk()
        {
            var dto = new RsvpUpdateDTO { Status = RSVPStatus.Maybe };
            var updated = new RsvpDTO { Id = 1, Status = RSVPStatus.Maybe };
            _mockService.Setup(s => s.UpdateRsvpAsync(1, dto)).ReturnsAsync(updated);

            var result = await _controller.Update(1, dto);
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(updated, ok.Value);
        }

        [Fact]
        public async Task Delete_Valid_ReturnsNoContent()
        {
            _mockService.Setup(s => s.DeleteRsvpAsync(1)).Returns(Task.CompletedTask);

            var result = await _controller.Delete(1);
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task UpdateStatus_Valid_ReturnsOk()
        {
            _mockService.Setup(s => s.UpdateStatusAsync(1, RSVPStatus.Declined)).ReturnsAsync(true);

            var result = await _controller.UpdateStatus(1, RSVPStatus.Declined);
            var ok = Assert.IsType<OkObjectResult>(result);

            // Deserialize to JsonElement for assertion
            var json = JsonSerializer.Serialize(ok.Value);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            Assert.True(root.TryGetProperty("success", out var successProp));
            Assert.True(successProp.GetBoolean());
        }

        [Fact]
        public async Task GetByEvent_ReturnsOk()
        {
            var rsvps = new List<RsvpDTO> { new RsvpDTO { Id = 1 } };
            _mockService.Setup(s => s.GetRsvpsByEventIdAsync(10)).ReturnsAsync(rsvps);

            var result = await _controller.GetByEvent(10);
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(rsvps, ok.Value);
        }

        [Fact]
        public async Task GetByUser_ReturnsOk()
        {
            var rsvps = new List<RsvpDTO> { new RsvpDTO { Id = 1 } };
            _mockService.Setup(s => s.GetRsvpsByUserIdAsync(5)).ReturnsAsync(rsvps);

            var result = await _controller.GetByUser(5);
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(rsvps, ok.Value);
        }

        [Fact]
        public async Task Cancel_Valid_ReturnsOk()
        {
            var resultDto = new RsvpDTO { Id = 1 };
            _mockService.Setup(s => s.CancelRsvpAsync(1)).ReturnsAsync(resultDto);

            var result = await _controller.Cancel(1);
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(resultDto, ok.Value);
        }

        [Fact]
        public async Task RespondToInvite_Valid_ReturnsOk()
        {
            var response = new RsvpDTO { Id = 1 };
            _mockService.Setup(s => s.RespondToInviteAsync(3, RSVPStatus.Accepted)).ReturnsAsync(response);

            var result = await _controller.RespondToInvite(3, RSVPStatus.Accepted);
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(response, ok.Value);
        }
    }
}
