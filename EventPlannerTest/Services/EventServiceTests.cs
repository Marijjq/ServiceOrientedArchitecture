using AutoMapper;
using EventPlanner.DTOs.Event;
using EventPlanner.Enums;
using EventPlanner.Models;
using EventPlanner.Repositories.Interfaces;
using EventPlanner.Services.Implementations;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventPlannerTest.Services
{
    public class EventServiceTests
    {
        private readonly Mock<IEventRepository> _mockEventRepository;
        private readonly Mock<ICategoryRepository> _mockCategoryRepository;
        private readonly Mock<IMapper> _mockMapper;
        private readonly EventService _eventService;

        public EventServiceTests()
        {
            _mockEventRepository = new Mock<IEventRepository>();
            _mockCategoryRepository = new Mock<ICategoryRepository>();
            _mockMapper = new Mock<IMapper>();

            _eventService = new EventService(
                _mockEventRepository.Object,
                _mockCategoryRepository.Object,
                _mockMapper.Object
            );
        }

        [Fact]
        public async Task GetEventByIdAsync_EventExists_ReturnsEventDto()
        {
            var eventId = 1;
            var eventModel = new Event { Id = eventId, Title = "Test Event" };
            var eventDto = new EventDTO { Id = eventId, Title = "Test Event" };

            _mockEventRepository.Setup(repo => repo.GetEventByIdAsync(eventId))
                .ReturnsAsync(eventModel);

            _mockMapper.Setup(m => m.Map<EventDTO>(eventModel))
                .Returns(eventDto);

            var result = await _eventService.GetEventByIdAsync(eventId);

            Assert.NotNull(result);
            Assert.Equal(eventDto.Title, result.Title);
            _mockEventRepository.Verify(repo => repo.GetEventByIdAsync(eventId), Times.Once);
        }

        [Fact]
        public async Task GetAllEventsAsync_ReturnsAllEvents()
        {
            var eventList = new List<Event>
        {
            new Event { Id = 1, Title = "Event 1" },
            new Event { Id = 2, Title = "Event 2" }
        };
            var dtoList = new List<EventDTO>
        {
            new EventDTO { Id = 1, Title = "Event 1" },
            new EventDTO { Id = 2, Title = "Event 2" }
        };

            _mockEventRepository.Setup(repo => repo.GetAllEventsAsync()).ReturnsAsync(eventList);
            _mockMapper.Setup(mapper => mapper.Map<IEnumerable<EventDTO>>(eventList)).Returns(dtoList);

            var result = await _eventService.GetAllEventsAsync();

            Assert.Equal(2, result.Count());
            _mockEventRepository.Verify(repo => repo.GetAllEventsAsync(), Times.Once);
            _mockMapper.Verify(mapper => mapper.Map<IEnumerable<EventDTO>>(eventList), Times.Once);
        }

        [Fact]
        public async Task CreateEventAsync_InvalidCategory_ThrowsArgumentException()
        {
            var dto = new EventCreateDTO
            {
                Title = "New Event",
                Location = "Location",
                StartDate = DateTime.Now.AddDays(1),
                EndDate = DateTime.Now.AddDays(2),
                MaxParticipants = 10,
                Status = EventStatus.Upcoming,
                CategoryId = 5
            };

            _mockCategoryRepository.Setup(repo => repo.GetCategoryByIdAsync(dto.CategoryId))
                .ReturnsAsync((Category)null);

            await Assert.ThrowsAsync<ArgumentException>(() => _eventService.CreateEventAsync(dto, userId: "1"));
        }

        [Fact]
        public async Task IsEventFullAsync_EventHasSpace_ReturnsFalse()
        {
            var eventId = 1;
            var eventModel = new Event { Id = eventId, MaxParticipants = 50 };

            _mockEventRepository.Setup(repo => repo.GetEventByIdAsync(eventId))
                .ReturnsAsync(eventModel);

            var result = await _eventService.IsEventFullAsync(eventId, 30);

            Assert.False(result);
        }

        [Fact]
        public async Task IsEventUpcomingAsync_UpcomingEvent_ReturnsTrue()
        {
            var eventId = 1;
            var futureDate = DateTime.UtcNow.AddDays(1);
            var eventModel = new Event
            {
                Id = eventId,
                Status = EventStatus.Upcoming,
                StartDate = futureDate
            };

            _mockEventRepository.Setup(repo => repo.GetEventByIdAsync(eventId))
                .ReturnsAsync(eventModel);

            var result = await _eventService.IsEventUpcomingAsync(eventId);

            Assert.True(result);
        }
    }
}
