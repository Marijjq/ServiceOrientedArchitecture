using EventPlanner.Data;
using EventPlanner.Models;
using EventPlanner.Repositories.Implementations;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.InMemory;

namespace EventPlannerTest.Repositories
{
    public class CategoryRepositoryTests : IDisposable
    {
        private readonly string _databaseName;
        private readonly DbContextOptions<ApplicationDbContext> _options;

        public CategoryRepositoryTests()
        {
            _databaseName = $"TestCategoryDb_{Guid.NewGuid()}";
            _options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: _databaseName) // This will now work
                .Options;

            // Seed data
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
            context.Categories.RemoveRange(context.Categories);
            context.SaveChanges();

            context.Categories.AddRange(
                new Category { Name = "Work", Description = "Work related events" },
                new Category { Name = "Personal", Description = "Personal events" }
            );

            context.SaveChanges();
        }

        [Fact]
        public async Task GetAllCategoriesAsync_ReturnsAllCategories()
        {
            using var context = new ApplicationDbContext(_options);
            var repository = new CategoryRepository(context);

            var categories = await repository.GetAllCategoriesAsync();

            Assert.NotNull(categories);
            var list = categories.ToList();
            Assert.Equal(2, list.Count);
            Assert.Contains(list, c => c.Name == "Work");
            Assert.Contains(list, c => c.Name == "Personal");
        }

        [Fact]
        public async Task GetCategoryByIdAsync_ReturnsCategory()
        {
            using var context = new ApplicationDbContext(_options);
            var repository = new CategoryRepository(context);

            var existingCategory = context.Categories.First();

            var category = await repository.GetCategoryByIdAsync(existingCategory.Id);

            Assert.NotNull(category);
            Assert.Equal(existingCategory.Name, category.Name);
        }

        [Fact]
        public async Task GetCategoryByIdAsync_NonExistingId_ReturnsNull()
        {
            using var context = new ApplicationDbContext(_options);
            var repository = new CategoryRepository(context);

            var category = await repository.GetCategoryByIdAsync(-1);

            Assert.Null(category);
        }

        [Fact]
        public async Task AddCategoryAsync_AddsCategory()
        {
            using var context = new ApplicationDbContext(_options);
            var repository = new CategoryRepository(context);

            var newCategory = new Category
            {
                Name = "Health",
                Description = "Health related events"
            };

            await repository.AddCategoryAsync(newCategory);

            var categoryInDb = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Health");
            Assert.NotNull(categoryInDb);
            Assert.Equal("Health", categoryInDb.Name);
        }

        [Fact]
        public async Task UpdateCategoryAsync_UpdatesCategory()
        {
            using var context = new ApplicationDbContext(_options);
            var repository = new CategoryRepository(context);

            var category = context.Categories.First();
            category.Description = "Updated description";

            await repository.UpdateCategoryAsync(category);

            var updatedCategory = await context.Categories.FindAsync(category.Id);
            Assert.Equal("Updated description", updatedCategory.Description);
        }

        [Fact]
        public async Task DeleteCategoryAsync_RemovesCategory()
        {
            using var context = new ApplicationDbContext(_options);
            var repository = new CategoryRepository(context);

            var category = context.Categories.First();

            await repository.DeleteCategoryAsync(category.Id);

            var deletedCategory = await context.Categories.FindAsync(category.Id);
            Assert.Null(deletedCategory);
        }

        [Fact]
        public async Task GetCategoryByNameAsync_ReturnsCategory()
        {
            using var context = new ApplicationDbContext(_options);
            var repository = new CategoryRepository(context);

            var category = await repository.GetCategoryByNameAsync("Work");

            Assert.NotNull(category);
            Assert.Equal("Work", category.Name);
        }

        [Fact]
        public async Task GetCategoryByNameAsync_NonExistingName_ReturnsNull()
        {
            using var context = new ApplicationDbContext(_options);
            var repository = new CategoryRepository(context);

            var category = await repository.GetCategoryByNameAsync("NonExistingName");

            Assert.Null(category);
        }

    }
}
