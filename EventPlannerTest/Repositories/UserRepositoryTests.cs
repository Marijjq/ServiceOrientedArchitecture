using EventPlanner.Data;
using EventPlanner.Models;
using EventPlanner.Repositories.Implementations;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventPlannerTest.Repositories
{
    public class UserRepositoryTests
    {
        private readonly string _databaseName;
        private readonly DbContextOptions<ApplicationDbContext> _options;

        public UserRepositoryTests()
        {
            _databaseName = $"TestUserDb_{Guid.NewGuid()}";
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
            context.Users.RemoveRange(context.Users);
            context.SaveChanges();

            var users = new List<ApplicationUser>
            {
                new ApplicationUser
                {
                    Id = "1",
                    Email = "alice@example.com",
                    FirstName = "Alice",
                    LastName = "Smith",
                    PhoneNumber = "123-456-7890"
                },
                new ApplicationUser
                {
                    Id = "2",
                    Email = "bob@example.com",
                    FirstName = "Bob",
                    LastName = "Johnson",
                    PhoneNumber = "234-567-8901"
                },
                new ApplicationUser
                {
                    Id = "3",
                    Email = "charlie@example.com",
                    FirstName = "Charlie",
                    LastName = "Williams",
                    PhoneNumber = "345-678-9012"
                }
            };

            context.Users.AddRange(users);
            context.SaveChanges();
        }

        [Fact]
        public async Task GetUserByIdAsync_ReturnsUser()
        {
            using var context = new ApplicationDbContext(_options);
            var repo = new UserRepository(context);

            var user = await repo.GetUserByIdAsync("1");

            Assert.NotNull(user);
            Assert.Equal("alice@example.com", user.Email);
        }

        [Fact]
        public async Task GetUserByIdAsync_ReturnsNull_WhenNotFound()
        {
            using var context = new ApplicationDbContext(_options);
            var repo = new UserRepository(context);

            var user = await repo.GetUserByIdAsync("999");

            Assert.Null(user);
        }

        [Fact]
        public async Task GetAllUsersAsync_ReturnsAllUsers()
        {
            using var context = new ApplicationDbContext(_options);
            var repo = new UserRepository(context);

            var users = await repo.GetAllUsersAsync();

            Assert.NotNull(users);
            Assert.Equal(3, users.Count());
            Assert.Contains(users, u => u.Email == "bob@example.com");
        }

        [Fact]
        public async Task AddUserAsync_AddsUserSuccessfully()
        {
            using var context = new ApplicationDbContext(_options);
            var repo = new UserRepository(context);

            var newUser = new ApplicationUser
            {
                Email = "dave@example.com",
                FirstName = "Dave",
                LastName = "Brown",
                PhoneNumber = "456-789-0123"
            };

            await repo.AddUserAsync(newUser);

            var added = await context.Users.FirstOrDefaultAsync(u => u.Email == "dave@example.com");
            Assert.NotNull(added);
            Assert.Equal("Dave", added.FirstName);
        }

        [Fact]
        public async Task UpdateUserAsync_UpdatesUserSuccessfully()
        {
            using var context = new ApplicationDbContext(_options);
            var repo = new UserRepository(context);

            var user = await context.Users.FindAsync("1");
            Assert.NotNull(user);

            user.FirstName = "AliceUpdated";
            await repo.UpdateUserAsync(user);

            var updated = await context.Users.FindAsync("1");
            Assert.NotNull(updated);
            Assert.Equal("AliceUpdated", updated.FirstName);
        }

        [Fact]
        public async Task DeleteUserAsync_RemovesUser()
        {
            using var context = new ApplicationDbContext(_options);
            var repo = new UserRepository(context);

            await repo.DeleteUserAsync("1");

            var deleted = await context.Users.FindAsync("1");
            Assert.Null(deleted);
        }

        [Fact]
        public async Task DeleteUserAsync_DoesNothing_WhenUserNotFound()
        {
            using var context = new ApplicationDbContext(_options);
            var repo = new UserRepository(context);

            // Should not throw
            await repo.DeleteUserAsync("999");
        }

        [Fact]
        public async Task GetUserByEmailAsync_ReturnsUser()
        {
            using var context = new ApplicationDbContext(_options);
            var repo = new UserRepository(context);

            var user = await repo.GetUserByEmailAsync("bob@example.com");

            Assert.NotNull(user);
            Assert.Equal("Bob", user.FirstName);
        }

        [Fact]
        public async Task GetUserByEmailAsync_ReturnsNull_WhenNotFound()
        {
            using var context = new ApplicationDbContext(_options);
            var repo = new UserRepository(context);

            var user = await repo.GetUserByEmailAsync("nonexistent@example.com");

            Assert.Null(user);
        }
    }
}