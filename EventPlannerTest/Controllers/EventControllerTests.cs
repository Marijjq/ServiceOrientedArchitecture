using EventPlanner.Controllers;
using EventPlanner.DTOs.Event;
using EventPlanner.Enums;
using EventPlanner.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace EventPlannerTest.Controllers
{
    public class EventControllerTests_NoAuth
    {
        private readonly Mock<IEventService> _mockService;
        private readonly EventController _controller;

        public EventControllerTests_NoAuth()
        {
            _mockService = new Mock<IEventService>();
            _controller = new EventController(_mockService.Object);

            // Setup User with "id" claim for methods that expect it
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim("id", "123")
            }, "mock"));
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext { User = user }
            };
        }

        [Fact]
        public async Task GetAllEvents_ReturnsOkWithEvents()
        {
            var events = new List<EventDTO> { new EventDTO { Id = 1 }, new EventDTO { Id = 2 } };
            _mockService.Setup(s => s.GetAllEventsAsync()).ReturnsAsync(events);

            var result = await _controller.GetAllEvents();

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedEvents = Assert.IsAssignableFrom<IEnumerable<EventDTO>>(okResult.Value);
            Assert.Equal(2, ((List<EventDTO>)returnedEvents).Count);
        }

        [Fact]
        public async Task GetEventById_ReturnsOk_WhenFound()
        {
            var eventItem = new EventDTO { Id = 5 };
            _mockService.Setup(s => s.GetEventByIdAsync(5)).ReturnsAsync(eventItem);

            var result = await _controller.GetEventById(5);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedEvent = Assert.IsType<EventDTO>(okResult.Value);
            Assert.Equal(5, returnedEvent.Id);
        }

        [Fact]
        public async Task GetEventById_ReturnsNotFound_WhenNotFound()
        {
            _mockService.Setup(s => s.GetEventByIdAsync(10)).ReturnsAsync((EventDTO)null);

            var result = await _controller.GetEventById(10);

            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Equal("Event not found.", notFoundResult.Value);
        }

        [Fact]
        public async Task CreateEvent_ReturnsCreatedAtAction_WhenSuccessful()
        {
            var createDto = new EventCreateDTO();
            var createdEvent = new EventDTO { Id = 1 };
            _mockService.Setup(s => s.CreateEventAsync(createDto, "123")).ReturnsAsync(createdEvent);

            var result = await _controller.CreateEvent(createDto);

            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(nameof(_controller.GetEventById), createdResult.ActionName);
            var returnedEvent = Assert.IsType<EventDTO>(createdResult.Value);
            Assert.Equal(1, returnedEvent.Id);
        }

        [Fact]
        public async Task CreateEvent_ReturnsUnauthorized_WhenUserIdClaimMissing()
        {
            // Remove user claims
            _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity());

            var createDto = new EventCreateDTO();

            var result = await _controller.CreateEvent(createDto);

            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result.Result);
            Assert.Equal("User ID not found.", unauthorizedResult.Value);
        }

        [Fact]
        public async Task CreateEvent_ReturnsBadRequest_OnArgumentException()
        {
            var createDto = new EventCreateDTO();
            _mockService.Setup(s => s.CreateEventAsync(createDto, "123")).ThrowsAsync(new ArgumentException("Invalid data"));

            var result = await _controller.CreateEvent(createDto);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("Invalid data", badRequest.Value);
        }

        [Fact]
        public async Task CreateEvent_ReturnsInternalServerError_OnException()
        {
            var createDto = new EventCreateDTO();
            _mockService.Setup(s => s.CreateEventAsync(createDto, "123")).ThrowsAsync(new Exception("DB failure"));

            var result = await _controller.CreateEvent(createDto);

            var statusResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, statusResult.StatusCode);
            Assert.Contains("DB failure", statusResult.Value.ToString());
        }

        [Fact]
        public async Task UpdateEvent_ReturnsOk_WhenSuccessful()
        {
            var updateDto = new EventUpdateDTO();
            var updatedEvent = new EventDTO { Id = 2 };
            _mockService.Setup(s => s.UpdateEventAsync(2, updateDto, "123")).ReturnsAsync(updatedEvent);

            var result = await _controller.UpdateEvent(2, updateDto);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedEvent = Assert.IsType<EventDTO>(okResult.Value);
            Assert.Equal(2, returnedEvent.Id);
        }

        [Fact]
        public async Task UpdateEvent_ReturnsUnauthorized_WhenUserIdClaimMissing()
        {
            _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity());

            var updateDto = new EventUpdateDTO();

            var result = await _controller.UpdateEvent(2, updateDto);

            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result.Result);
            Assert.Equal("User ID not found.", unauthorizedResult.Value);
        }

        [Fact]
        public async Task UpdateEvent_ReturnsBadRequest_OnArgumentException()
        {
            var updateDto = new EventUpdateDTO();
            _mockService.Setup(s => s.UpdateEventAsync(2, updateDto, "123")).ThrowsAsync(new ArgumentException("Bad data"));

            var result = await _controller.UpdateEvent(2, updateDto);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("Bad data", badRequest.Value);
        }

        [Fact]
        public async Task UpdateEvent_ReturnsForbid_OnUnauthorizedAccessException()
        {
            var updateDto = new EventUpdateDTO();
            _mockService.Setup(s => s.UpdateEventAsync(2, updateDto, "123")).ThrowsAsync(new UnauthorizedAccessException());

            var result = await _controller.UpdateEvent(2, updateDto);

            var forbidResult = Assert.IsType<ForbidResult>(result.Result);
        }

        [Fact]
        public async Task UpdateEvent_ReturnsInternalServerError_OnException()
        {
            var updateDto = new EventUpdateDTO();
            _mockService.Setup(s => s.UpdateEventAsync(2, updateDto, "123")).ThrowsAsync(new Exception("DB failure"));

            var result = await _controller.UpdateEvent(2, updateDto);

            var statusResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, statusResult.StatusCode);
            Assert.Contains("DB failure", statusResult.Value.ToString());
        }

        [Fact]
        public async Task DeleteEvent_ReturnsNoContent_WhenSuccessful()
        {
            _mockService.Setup(s => s.DeleteEventAsync(5)).Returns(Task.CompletedTask);

            var result = await _controller.DeleteEvent(5);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteEvent_ReturnsNotFound_OnArgumentException()
        {
            _mockService.Setup(s => s.DeleteEventAsync(5)).ThrowsAsync(new ArgumentException("Not found"));

            var result = await _controller.DeleteEvent(5);

            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Not found", notFoundResult.Value);
        }

        [Fact]
        public async Task DeleteEvent_ReturnsInternalServerError_OnException()
        {
            _mockService.Setup(s => s.DeleteEventAsync(5)).ThrowsAsync(new Exception("DB error"));

            var result = await _controller.DeleteEvent(5);

            var statusResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusResult.StatusCode);
            Assert.Contains("DB error", statusResult.Value.ToString());
        }

        [Fact]
        public async Task GetEventsByUserId_ReturnsOkWithEvents()
        {
            var events = new List<EventDTO> { new EventDTO { Id = 7 } };
            _mockService.Setup(s => s.GetEventsByUserIdAsync("7")).ReturnsAsync(events);

            var result = await _controller.GetEventsByUserId("7");

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedEvents = Assert.IsAssignableFrom<IEnumerable<EventDTO>>(okResult.Value);
            Assert.Single(returnedEvents);
        }

        [Fact]
        public async Task GetEventsByCategoryId_ReturnsOkWithEvents()
        {
            var events = new List<EventDTO> { new EventDTO { Id = 9 } };
            _mockService.Setup(s => s.GetEventsByCategoryIdAsync(9)).ReturnsAsync(events);

            var result = await _controller.GetEventsByCategoryId(9);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedEvents = Assert.IsAssignableFrom<IEnumerable<EventDTO>>(okResult.Value);
            Assert.Single(returnedEvents);
        }

        [Fact]
        public async Task GetEventsByStatus_ReturnsOkWithEvents()
        {
            var events = new List<EventDTO> { new EventDTO { Id = 11 } };
            _mockService.Setup(s => s.GetEventsByStatusAsync(EventStatus.Upcoming)).ReturnsAsync(events);
            var result = await _controller.GetEventsByStatus(EventStatus.Upcoming);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedEvents = Assert.IsAssignableFrom<IEnumerable<EventDTO>>(okResult.Value);
            Assert.Single(returnedEvents);
        }

        [Fact]
        public async Task ToggleEventStatus_ReturnsNoContent_WhenSuccessful()
        {
            _mockService.Setup(s => s.ToggleEventStatusAsync(5, EventStatus.Cancelled, "123")).Returns(Task.CompletedTask);

            var result = await _controller.ToggleEventStatus(5, EventStatus.Cancelled);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task GetUpcomingEvents_ReturnsOkWithEvents()
        {
            var events = new List<EventDTO> { new EventDTO { Id = 14 } };
            _mockService.Setup(s => s.GetUpcomingEventsAsync()).ReturnsAsync(events);

            var result = await _controller.GetUpcomingEvents();

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedEvents = Assert.IsAssignableFrom<IEnumerable<EventDTO>>(okResult.Value);
            Assert.Single(returnedEvents);
        }

        [Fact]
        public async Task SearchEventsByTitle_ReturnsOkWithResults()
        {
            var events = new List<EventDTO> { new EventDTO { Id = 22 } };
            _mockService.Setup(s => s.SearchEventsByTitleAsync("conference")).ReturnsAsync(events);

            var result = await _controller.SearchEventsByTitle("conference");

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedEvents = Assert.IsAssignableFrom<IEnumerable<EventDTO>>(okResult.Value);
            Assert.Single(returnedEvents);
        }
    }
}