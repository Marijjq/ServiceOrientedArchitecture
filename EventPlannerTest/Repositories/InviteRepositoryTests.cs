using EventPlanner.Data;
using EventPlanner.Enums;
using EventPlanner.Models;
using EventPlanner.Repositories.Implementations;
using Microsoft.EntityFrameworkCore;
using Xunit;
using System;
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
            _databaseName = $"TestInviteDb_{Guid.NewGuid()}";
            _options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(_databaseName)
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
            using var context = new ApplicationDbContext(_options);
            context.Database.EnsureDeleted();
        }

        private void SeedDatabase(ApplicationDbContext context)
        {
            // Clear existing data
            context.Invites.RemoveRange(context.Invites);
            context.Users.RemoveRange(context.Users);
            context.Events.RemoveRange(context.Events);
            context.SaveChanges();

            // Seed users
            var inviter = new ApplicationUser
            {
                Id = "1",
                Email = "inviter@example.com",
                FirstName = "Inviter",
                LastName = "User",
                PhoneNumber = "111-111-1111"
            };
            var invitee1 = new ApplicationUser
            {
                Id = "2",
                Email = "invitee1@example.com",
                FirstName = "Invitee",
                LastName = "One",
                PhoneNumber = "222-222-2222"
            };
            var invitee2 = new ApplicationUser
            {
                Id = "3",
                Email = "invitee2@example.com",
                FirstName = "Invitee",
                LastName = "Two",
                PhoneNumber = "333-333-3333"
            };
            context.Users.AddRange(inviter, invitee1, invitee2);

            // Seed events
            var event1 = new Event
            {
                Id = 1,
                Title = "Event 1",
                Description = "Description 1",
                Location = "Location 1",
                StartDate = DateTime.UtcNow.AddDays(1),
                EndDate = DateTime.UtcNow.AddDays(2),
                UserId = inviter.Id,
                CategoryId = 1,
                Status = EventStatus.Upcoming,
                MaxParticipants = 100
            };
            var event2 = new Event
            {
                Id = 2,
                Title = "Event 2",
                Description = "Description 2",
                Location = "Location 2",
                StartDate = DateTime.UtcNow.AddDays(3),
                EndDate = DateTime.UtcNow.AddDays(4),
                UserId = inviter.Id,
                CategoryId = 1,
                Status = EventStatus.Upcoming,
                MaxParticipants = 100
            };
            context.Events.AddRange(event1, event2);

            context.SaveChanges();

            // Seed invites
            var invite1 = new Invite
            {
                Id = 1,
                InviterId = inviter.Id,
                InviteeId = invitee1.Id,
                EventId = event1.Id,
                Status = InviteStatus.Pending,
                SentAt = DateTime.UtcNow
            };

            var invite2 = new Invite
            {
                Id = 2,
                InviterId = inviter.Id,
                InviteeId = invitee2.Id,
                EventId = event2.Id,
                Status = InviteStatus.Accepted,
                SentAt = DateTime.UtcNow
            };

            context.Invites.AddRange(invite1, invite2);
            context.SaveChanges();
        }

        [Fact]
        public async Task GetInviteByIdAsync_ReturnsInviteWithIncludes()
        {
            using var context = new ApplicationDbContext(_options);
            var repo = new InviteRepository(context);

            var invite = await repo.GetInviteByIdAsync(1);

            Assert.NotNull(invite);
            Assert.Equal(1, invite.Id);
            Assert.NotNull(invite.Inviter);
            Assert.NotNull(invite.Invitee);
            Assert.NotNull(invite.Event);
            Assert.Equal(InviteStatus.Pending, invite.Status);
        }

        [Fact]
        public async Task GetAllInvitesAsync_ReturnsAllInvitesWithIncludes()
        {
            using var context = new ApplicationDbContext(_options);
            var repo = new InviteRepository(context);

            var invites = await repo.GetAllInvitesAsync();
            var list = invites.ToList();

            Assert.Equal(2, list.Count);
            Assert.All(list, i => Assert.NotNull(i.Invitee));
            Assert.All(list, i => Assert.NotNull(i.Event));
        }

        [Fact]
        public async Task AddInviteAsync_AddsInviteSuccessfully()
        {
            using var context = new ApplicationDbContext(_options);
            var repo = new InviteRepository(context);

            var newInvite = new Invite
            {
                InviterId = "1",
                InviteeId = "3",
                EventId = 1,
                Status = InviteStatus.Pending,
                SentAt = DateTime.UtcNow
            };

            await repo.AddInviteAsync(newInvite);

            var inviteInDb = await context.Invites.FindAsync(newInvite.Id);
            Assert.NotNull(inviteInDb);
            Assert.Equal("3", inviteInDb.InviteeId);
        }

        [Fact]
        public async Task UpdateInviteAsync_UpdatesInviteSuccessfully()
        {
            using var context = new ApplicationDbContext(_options);
            var repo = new InviteRepository(context);

            var invite = await context.Invites.FirstAsync();
            invite.Status = InviteStatus.Declined;

            await repo.UpdateInviteAsync(invite);

            var updatedInvite = await context.Invites.FindAsync(invite.Id);
            Assert.Equal(InviteStatus.Declined, updatedInvite.Status);
        }

        [Fact]
        public async Task DeleteInviteAsync_RemovesInvite()
        {
            using var context = new ApplicationDbContext(_options);
            var repo = new InviteRepository(context);

            await repo.DeleteInviteAsync(1);

            var deletedInvite = await context.Invites.FindAsync(1);
            Assert.Null(deletedInvite);
        }

        [Fact]
        public async Task GetPendingInvitesByUserIdAsync_ReturnsOnlyPending()
        {
            using var context = new ApplicationDbContext(_options);
            var repo = new InviteRepository(context);

            var pendingInvites = await repo.GetPendingInvitesByUserIdAsync("2");

            var list = pendingInvites.ToList();
            Assert.Single(list);
            Assert.All(list, i => Assert.Equal(InviteStatus.Pending, i.Status));
            Assert.All(list, i => Assert.Equal("2", i.InviteeId));
        }

        [Fact]
        public async Task GetByInviteeAndEventAsync_ReturnsCorrectInvite()
        {
            using var context = new ApplicationDbContext(_options);
            var repo = new InviteRepository(context);

            var invite = await repo.GetByInviteeAndEventAsync("2", 1);

            Assert.NotNull(invite);
            Assert.Equal("2", invite.InviteeId);
            Assert.Equal(1, invite.EventId);
        }

        [Fact]
        public async Task GetByInviteeAndEventAsync_ReturnsNullIfNotFound()
        {
            using var context = new ApplicationDbContext(_options);
            var repo = new InviteRepository(context);

            var invite = await repo.GetByInviteeAndEventAsync("999", 999);

            Assert.Null(invite);
        }
    }
}
