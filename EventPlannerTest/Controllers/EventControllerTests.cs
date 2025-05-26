using Xunit;
using Moq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using EventPlanner.Controllers;
using EventPlanner.Services.Interfaces;
using EventPlanner.DTOs.Event;
using EventPlanner.Enums;

namespace EventPlannerTest.Controllers
{
    public class EventControllerTests
    {
        private readonly Mock<IEventService> _eventServiceMock;
        private readonly EventController _controller;

        public EventControllerTests()
        {
            _eventServiceMock = new Mock<IEventService>();
            _controller = new EventController(_eventServiceMock.Object);
        }

        [Fact]
        public async Task GetAllEvents_ReturnsOkResult_WithListOfEvents()
        {
            _eventServiceMock.Setup(s => s.GetAllEventsAsync())
                .ReturnsAsync(new List<EventDTO> { new EventDTO { Id = 1, Title = "Test Event" } });

            var result = await _controller.GetAllEvents();

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var events = Assert.IsAssignableFrom<IEnumerable<EventDTO>>(ok.Value);
            Assert.Single(events);
        }

        [Fact]
        public async Task GetEventById_ValidId_ReturnsEvent()
        {
            var dto = new EventDTO { Id = 1, Title = "Test" };
            _eventServiceMock.Setup(s => s.GetEventByIdAsync(1)).ReturnsAsync(dto);

            var result = await _controller.GetEventById(1);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var ev = Assert.IsType<EventDTO>(ok.Value);
            Assert.Equal("Test", ev.Title);
        }

        [Fact]
        public async Task GetEventById_InvalidId_ReturnsNotFound()
        {
            _eventServiceMock.Setup(s => s.GetEventByIdAsync(999)).ReturnsAsync((EventDTO)null);

            var result = await _controller.GetEventById(999);

            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public async Task CreateEvent_Valid_ReturnsCreated()
        {
            var createDto = new EventCreateDTO { Title = "New Event", Location = "Tetovo" };
            var createdDto = new EventDTO { Id = 1, Title = "New Event" };

            _eventServiceMock.Setup(s => s.CreateEventAsync(createDto, 1))
                .ReturnsAsync(createdDto);

            var result = await _controller.CreateEvent(createDto);

            var created = Assert.IsType<CreatedAtActionResult>(result.Result);
            var ev = Assert.IsType<EventDTO>(created.Value);
            Assert.Equal("New Event", ev.Title);
        }

        [Fact]
        public async Task UpdateEvent_Valid_ReturnsOk()
        {
            var updateDto = new EventUpdateDTO { Title = "Updated" };
            var updatedDto = new EventDTO { Id = 1, Title = "Updated" };

            _eventServiceMock.Setup(s => s.UpdateEventAsync(1, updateDto, 1))
                .ReturnsAsync(updatedDto);

            var result = await _controller.UpdateEvent(1, updateDto);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var ev = Assert.IsType<EventDTO>(ok.Value);
            Assert.Equal("Updated", ev.Title);
        }

        [Fact]
        public async Task UpdateEvent_Invalid_ReturnsBadRequest()
        {
            var updateDto = new EventUpdateDTO { Title = "" };

            _eventServiceMock.Setup(s => s.UpdateEventAsync(1, updateDto, 1))
                .ThrowsAsync(new ArgumentException("Invalid update."));

            var result = await _controller.UpdateEvent(1, updateDto);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("Invalid update.", badRequest.Value);
        }

        [Fact]
        public async Task UpdateEvent_Unauthorized_ReturnsForbid()
        {
            var updateDto = new EventUpdateDTO { Title = "Unauthorized" };

            _eventServiceMock.Setup(s => s.UpdateEventAsync(1, updateDto, 1))
                .ThrowsAsync(new UnauthorizedAccessException());

            var result = await _controller.UpdateEvent(1, updateDto);

            Assert.IsType<ForbidResult>(result.Result);
        }

        [Fact]
        public async Task DeleteEvent_Valid_ReturnsNoContent()
        {
            _eventServiceMock.Setup(s => s.DeleteEventAsync(1)).Returns(Task.CompletedTask);

            var result = await _controller.DeleteEvent(1);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteEvent_InvalidId_ReturnsNotFound()
        {
            _eventServiceMock.Setup(s => s.DeleteEventAsync(999))
                .ThrowsAsync(new ArgumentException("Event not found."));

            var result = await _controller.DeleteEvent(999);

            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Event not found.", notFound.Value);
        }

        [Fact]
        public async Task GetEventsByUserId_ReturnsEvents()
        {
            _eventServiceMock.Setup(s => s.GetEventsByUserIdAsync(1))
                .ReturnsAsync(new List<EventDTO> { new EventDTO { Id = 1 } });

            var result = await _controller.GetEventsByUserId(1);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var events = Assert.IsAssignableFrom<IEnumerable<EventDTO>>(ok.Value);
            Assert.Single(events);
        }

        [Fact]
        public async Task GetEventsByCategoryId_ReturnsEvents()
        {
            _eventServiceMock.Setup(s => s.GetEventsByCategoryIdAsync(1))
                .ReturnsAsync(new List<EventDTO> { new EventDTO { Id = 1 } });

            var result = await _controller.GetEventsByCategoryId(1);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Single((IEnumerable<EventDTO>)ok.Value);
        }

        [Fact]
        public async Task GetEventsByStatus_ReturnsEvents()
        {
            _eventServiceMock.Setup(s => s.GetEventsByStatusAsync(EventStatus.Upcoming))
                .ReturnsAsync(new List<EventDTO> { new EventDTO { Status = EventStatus.Upcoming.ToString() } });
            var result = await _controller.GetEventsByStatus(EventStatus.Upcoming);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Single((IEnumerable<EventDTO>)ok.Value);
        }

        [Fact]
        public async Task ToggleEventStatus_Valid_ReturnsNoContent()
        {
            _eventServiceMock.Setup(s => s.ToggleEventStatusAsync(1, EventStatus.Completed))
                .Returns(Task.CompletedTask);

            var result = await _controller.ToggleEventStatus(1, EventStatus.Completed);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task GetUpcomingEvents_ReturnsList()
        {
            _eventServiceMock.Setup(s => s.GetUpcomingEventsAsync())
                .ReturnsAsync(new List<EventDTO> { new EventDTO { Title = "Soon" } });

            var result = await _controller.GetUpcomingEvents();

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Single((IEnumerable<EventDTO>)ok.Value);
        }

        [Fact]
        public async Task SearchEventsByTitle_ReturnsMatching()
        {
            _eventServiceMock.Setup(s => s.SearchEventsByTitleAsync("party"))
                .ReturnsAsync(new List<EventDTO> { new EventDTO { Title = "Birthday party" } });

            var result = await _controller.SearchEventsByTitle("party");

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var list = Assert.IsAssignableFrom<IEnumerable<EventDTO>>(ok.Value);
            Assert.Contains(list, e => e.Title.Contains("party"));
        }
    }
}
