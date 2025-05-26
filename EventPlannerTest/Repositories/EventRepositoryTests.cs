using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using EventPlanner.Data;
using EventPlanner.Models;
using EventPlanner.Repositories.Implementations;
using EventPlanner.Enums;
using Xunit;

namespace EventPlanner.Tests.Repositories
{
    public class EventRepositoryTests : IDisposable
    {
        private readonly string _databaseName;
        private readonly DbContextOptions<ApplicationDbContext> _options;

        public EventRepositoryTests()
        {
            _databaseName = $"TestEventDatabase_{Guid.NewGuid()}";
            _options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: _databaseName)
                .Options;

            using (var context = new ApplicationDbContext(_options))
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();
                SeedDatabase(context);
            }
        }

        public void Dispose()
        {
            using (var context = new ApplicationDbContext(_options))
            {
                context.Database.EnsureDeleted();
            }
        }

        private void SeedDatabase(ApplicationDbContext context)
        {
            context.Events.RemoveRange(context.Events);
            context.SaveChanges();

            context.Events.AddRange(
                new Event
                {
                    Id = 1,
                    Title = "Event 1",
                    Description = "Description 1",
                    Location = "Location 1",
                    StartDate = DateTime.UtcNow.AddDays(7),
                    EndDate = DateTime.UtcNow.AddDays(8),
                    UserId = 1,
                    CategoryId = 10,
                    Status = EventStatus.Upcoming
                },
                new Event
                {
                    Id = 2,
                    Title = "Event 2",
                    Description = "Description 2",
                    Location = "Location 2",
                    StartDate = DateTime.UtcNow.AddDays(7),
                    EndDate = DateTime.UtcNow.AddDays(8),
                    UserId = 2,
                    CategoryId = 20,
                    Status = EventStatus.Completed
                },
                new Event
                {
                    Id = 3,
                    Title = "Event 3",
                    Description = "Description 3",
                    Location = "Location 3",
                    StartDate = DateTime.UtcNow.AddDays(7),
                    EndDate = DateTime.UtcNow.AddDays(8),
                    UserId = 1,
                    CategoryId = 10,
                    Status = EventStatus.Cancelled
                }
            );

            context.SaveChanges();
        }

        [Fact]
        public async Task GetAllEventsAsync_ReturnsAllEvents()
        {
            using var context = new ApplicationDbContext(_options);
            var repository = new EventRepository(context);

            var events = (await repository.GetAllEventsAsync()).ToList();

            Assert.Equal(3, events.Count);
            Assert.Contains(events, e => e.Title == "Event 1");
            Assert.Contains(events, e => e.Title == "Event 2");
            Assert.Contains(events, e => e.Title == "Event 3");
        }

        [Fact]
        public async Task GetEventByIdAsync_ReturnsCorrectEvent()
        {
            using var context = new ApplicationDbContext(_options);
            var repository = new EventRepository(context);

            var eventItem = await repository.GetEventByIdAsync(2);

            Assert.NotNull(eventItem);
            Assert.Equal("Event 2", eventItem.Title);
            Assert.Equal(EventStatus.Completed, eventItem.Status);
        }

        [Fact]
        public async Task GetEventsByUserIdAsync_ReturnsCorrectEvents()
        {
            using var context = new ApplicationDbContext(_options);
            var repository = new EventRepository(context);

            var user1Events = (await repository.GetEventsByUserIdAsync(1)).ToList();

            Assert.Equal(2, user1Events.Count);
            Assert.All(user1Events, e => Assert.Equal(1, e.UserId));
        }

        [Fact]
        public async Task GetEventsByCategoryIdAsync_ReturnsCorrectEvents()
        {
            using var context = new ApplicationDbContext(_options);
            var repository = new EventRepository(context);

            var category10Events = (await repository.GetEventsByCategoryIdAsync(10)).ToList();

            Assert.Equal(2, category10Events.Count);
            Assert.All(category10Events, e => Assert.Equal(10, e.CategoryId));
        }

        [Fact]
        public async Task GetEventsByStatusAsync_ReturnsCorrectEvents()
        {
            using var context = new ApplicationDbContext(_options);
            var repository = new EventRepository(context);

            var upcomingEvents = (await repository.GetEventsByStatusAsync(EventStatus.Upcoming)).ToList();

            Assert.Single(upcomingEvents);
            Assert.All(upcomingEvents, e => Assert.Equal(EventStatus.Upcoming, e.Status));
        }

        [Fact]
        public async Task AddEventAsync_AddsEvent()
        {
            using var context = new ApplicationDbContext(_options);
            var repository = new EventRepository(context);

            var newEvent = new Event
            {
                Id = 4,
                Title = "Event 4",
                Description = "Description 4",
                Location = "Location 4",
                StartDate = DateTime.UtcNow.AddDays(7),
                EndDate = DateTime.UtcNow.AddDays(8),
                UserId = 3,
                CategoryId = 30,
                Status = EventStatus.Scheduled
            };

            await repository.AddEventAsync(newEvent);

            var allEvents = (await repository.GetAllEventsAsync()).ToList();

            Assert.Equal(4, allEvents.Count);
            Assert.Contains(allEvents, e => e.Title == "Event 4");
        }

        [Fact]
        public async Task UpdateEventAsync_UpdatesEvent()
        {
            using var context = new ApplicationDbContext(_options);
            var repository = new EventRepository(context);

            var eventToUpdate = await repository.GetEventByIdAsync(1);
            eventToUpdate.Status = EventStatus.Ongoing;

            await repository.UpdateEventAsync(eventToUpdate);

            var updatedEvent = await repository.GetEventByIdAsync(1);
            Assert.Equal(EventStatus.Ongoing, updatedEvent.Status);
        }

        [Fact]
        public async Task DeleteEventAsync_DeletesEvent()
        {
            using var context = new ApplicationDbContext(_options);
            var repository = new EventRepository(context);

            await repository.DeleteEventAsync(3);

            var allEvents = (await repository.GetAllEventsAsync()).ToList();
            Assert.Equal(2, allEvents.Count);
            Assert.DoesNotContain(allEvents, e => e.Id == 3);
        }
    }
}
