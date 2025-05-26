using EventPlanner.Data;
using EventPlanner.Enums;
using EventPlanner.Models;
using EventPlanner.Repositories.Implementations;
using Microsoft.EntityFrameworkCore;
using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EventPlanner.Tests.Repositories
{
    public class InviteRepositoryTests : IDisposable
    {
        private readonly string _databaseName;
        private readonly DbContextOptions<ApplicationDbContext> _options;

        public InviteRepositoryTests()
        {
            _databaseName = Guid.NewGuid().ToString(); 
            _options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(_databaseName)
                .Options;

            using var context = new ApplicationDbContext(_options);
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
            SeedDatabase(context);

        }

        public void Dispose()
        {
            using var context = new ApplicationDbContext(_options);
            context.Database.EnsureDeleted();
        }

        private void SeedDatabase(ApplicationDbContext context)
        {
            var user1 = new User
            {
                Id = 1,
                Username = "Alice",
                Email = "alice@example.com",
                Password = "TestPassword1!",
                FirstName = "Alice",
                LastName = "Smith",
                Role = "User",
                PhoneNumber = "123-456-7890"
            };
            var user2 = new User
            {
                Id = 2,
                Username = "Bob",
                Email = "bob@example.com",
                Password = "TestPassword2!",
                FirstName = "Bob",
                LastName = "Johnson",
                Role = "User",
                PhoneNumber = "987-654-3210"
            };
            var event1 = new Event
            {
                Id = 100,
                Title = "Test Event",
                UserId = 1,
                CategoryId = 1,
                Status = EventStatus.Upcoming,
                Location = "Test Location",
                StartDate = DateTime.UtcNow.AddDays(1),
                EndDate = DateTime.UtcNow.AddDays(2)
            };

            context.Users.AddRange(user1, user2);
            context.Events.Add(event1);

            context.Invites.Add(new Invite
            {
                Id = 1,
                InviterId = 1,
                InviteeId = 2,
                EventId = 100,
                Status = InviteStatus.Pending,
                Message = "Initial Invite"
            });

            context.SaveChanges();
        }

        [Fact]
        public async Task AddInviteAsync_AddsInvite_WithDefaults()
        {
            using var context = new ApplicationDbContext(_options);
            var repo = new InviteRepository(context);

            var newInvite = new Invite
            {
                InviterId = 1,
                InviteeId = 2,
                EventId = 100,
                Message = "You're invited to the event!"
            };

            await repo.AddInviteAsync(newInvite);

            var added = await context.Invites.FirstOrDefaultAsync(i => i.Message == "You're invited to the event!");

            Assert.NotNull(added);
            Assert.Equal(InviteStatus.Pending, added.Status);
            Assert.NotEqual(default, added.SentAt);
            Assert.Null(added.RespondedAt);
            Assert.Null(added.ExpiresAt);
        }

        [Fact]
        public async Task GetInviteByIdAsync_ReturnsCorrectInvite()
        {
            using var context = new ApplicationDbContext(_options);
            var repo = new InviteRepository(context);

            var invite = await repo.GetInviteByIdAsync(1);

            Assert.NotNull(invite);
            Assert.Equal(1, invite.InviterId);
            Assert.Equal(2, invite.InviteeId);
            Assert.Equal(100, invite.EventId);
            Assert.Equal("Initial Invite", invite.Message);
        }

        [Fact]
        public async Task GetAllInvitesAsync_ReturnsAll()
        {
            using var context = new ApplicationDbContext(_options);
            var repo = new InviteRepository(context);

            var all = await repo.GetAllInvitesAsync();

            Assert.Single(all);
        }

        [Fact]
        public async Task UpdateInviteAsync_ChangesStatusAndResponseTime()
        {
            using var context = new ApplicationDbContext(_options);
            var repo = new InviteRepository(context);

            var invite = await repo.GetInviteByIdAsync(1);
            invite.Status = InviteStatus.Accepted;
            invite.RespondedAt = DateTime.UtcNow;

            await repo.UpdateInviteAsync(invite);

            var updated = await context.Invites.FindAsync(1);

            Assert.Equal(InviteStatus.Accepted, updated.Status);
            Assert.NotNull(updated.RespondedAt);
        }

        [Fact]
        public async Task DeleteInviteAsync_RemovesInvite()
        {
            using var context = new ApplicationDbContext(_options);
            var repo = new InviteRepository(context);

            await repo.DeleteInviteAsync(1);
            var deleted = await context.Invites.FindAsync(1);

            Assert.Null(deleted);
        }

        [Fact]
        public async Task GetPendingInvitesByUserIdAsync_ReturnsCorrect()
        {
            using var context = new ApplicationDbContext(_options);
            var repo = new InviteRepository(context);

            var invites = await repo.GetPendingInvitesByUserIdAsync(2);

            Assert.Single(invites);
            Assert.Equal(InviteStatus.Pending, invites.First().Status);
        }

        [Fact]
        public async Task GetByInviteeAndEventAsync_ReturnsExpected()
        {
            using var context = new ApplicationDbContext(_options);
            var repo = new InviteRepository(context);

            var invite = await repo.GetByInviteeAndEventAsync(2, 100);

            Assert.NotNull(invite);
            Assert.Equal(1, invite.InviterId);
        }
    }
}
