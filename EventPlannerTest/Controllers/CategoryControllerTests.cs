using EventPlanner.Controllers;
using EventPlanner.DTOs.Category;
using EventPlanner.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;

namespace EventPlannerTest.Controllers
{
    public class CategoryControllerTests
    {
        private readonly Mock<ICategoryService> _mockService;
        private readonly CategoryController _controller;

        public CategoryControllerTests()
        {
            _mockService = new Mock<ICategoryService>();
            _controller = new CategoryController(_mockService.Object);

            // Simulate an authenticated Admin user
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, "admin"),
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Role, "Admin")
            }, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        [Fact]
        public async Task GetAllCategories_ReturnsOk_WithListOfCategories()
        {
            var categories = new List<CategoryDTO>
            {
                new CategoryDTO { Id = 1, Name = "Music" },
                new CategoryDTO { Id = 2, Name = "Sports" }
            };

            _mockService.Setup(s => s.GetAllCategoriesAsync()).ReturnsAsync(categories);

            var result = await _controller.GetAllCategories();

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnCategories = Assert.IsAssignableFrom<IEnumerable<CategoryDTO>>(okResult.Value);
            Assert.Equal(2, ((List<CategoryDTO>)returnCategories).Count);
        }

        [Fact]
        public async Task GetCategoryById_ExistingId_ReturnsOk()
        {
            var category = new CategoryDTO { Id = 1, Name = "Tech" };
            _mockService.Setup(s => s.GetCategoryByIdAsync(1)).ReturnsAsync(category);

            var result = await _controller.GetCategoryById(1);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedCategory = Assert.IsType<CategoryDTO>(okResult.Value);
            Assert.Equal("Tech", returnedCategory.Name);
        }

        [Fact]
        public async Task GetCategoryById_NonExistingId_ReturnsNotFound()
        {
            _mockService.Setup(s => s.GetCategoryByIdAsync(99)).ReturnsAsync((CategoryDTO)null);

            var result = await _controller.GetCategoryById(99);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GetCategoryByName_ExistingName_ReturnsOk()
        {
            var category = new CategoryDTO { Id = 1, Name = "Tech" };
            _mockService.Setup(s => s.GetCategoryByNameAsync("Tech")).ReturnsAsync(category);

            var result = await _controller.GetCategoryByName("Tech");

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedCategory = Assert.IsType<CategoryDTO>(okResult.Value);
            Assert.Equal("Tech", returnedCategory.Name);
        }

        [Fact]
        public async Task GetCategoryByName_NonExistingName_ReturnsNotFound()
        {
            _mockService.Setup(s => s.GetCategoryByNameAsync("Unknown")).ReturnsAsync((CategoryDTO)null);

            var result = await _controller.GetCategoryByName("Unknown");

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task CreateCategory_ValidCategory_ReturnsCreatedAtAction()
        {
            var createDto = new CategoryCreateDTO { Name = "Health" };
            var createdDto = new CategoryDTO { Id = 3, Name = "Health" };

            _mockService.Setup(s => s.CreateCategoryAsync(createDto)).ReturnsAsync(createdDto);

            var result = await _controller.CreateCategory(createDto);

            var createdAt = Assert.IsType<CreatedAtActionResult>(result.Result);
            var returnedCategory = Assert.IsType<CategoryDTO>(createdAt.Value);
            Assert.Equal("Health", returnedCategory.Name);
        }

        [Fact]
        public async Task CreateCategory_NullCategory_ReturnsBadRequest()
        {
            var result = await _controller.CreateCategory(null);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("Category cannot be null.", badRequest.Value);
        }

        [Fact]
        public async Task CreateCategory_ThrowsArgumentException_ReturnsBadRequest()
        {
            var createDto = new CategoryCreateDTO { Name = "Duplicate" };
            _mockService.Setup(s => s.CreateCategoryAsync(createDto)).ThrowsAsync(new ArgumentException("Duplicate name."));

            var result = await _controller.CreateCategory(createDto);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("Duplicate name.", badRequest.Value);
        }

        [Fact]
        public async Task UpdateCategory_ValidUpdate_ReturnsNoContent()
        {
            var updateDto = new CategoryUpdateDTO { Name = "Updated" };
            _mockService.Setup(s => s.UpdateCategoryAsync(1, updateDto)).ReturnsAsync(new CategoryDTO());

            var result = await _controller.UpdateCategory(1, updateDto);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task UpdateCategory_NonExistingId_ReturnsNotFound()
        {
            var updateDto = new CategoryUpdateDTO { Name = "Updated" };
            _mockService.Setup(s => s.UpdateCategoryAsync(99, updateDto)).ReturnsAsync((CategoryDTO)null);

            var result = await _controller.UpdateCategory(99, updateDto);

            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Category not found.", notFound.Value);
        }

        [Fact]
        public async Task UpdateCategory_ThrowsArgumentException_ReturnsNotFound()
        {
            var updateDto = new CategoryUpdateDTO { Name = "Error" };
            _mockService.Setup(s => s.UpdateCategoryAsync(1, updateDto)).ThrowsAsync(new ArgumentException("Error message"));

            var result = await _controller.UpdateCategory(1, updateDto);

            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Error message", notFound.Value);
        }

        [Fact]
        public async Task DeleteCategory_ValidId_ReturnsNoContent()
        {
            var result = await _controller.DeleteCategory(1);

            Assert.IsType<NoContentResult>(result);
            _mockService.Verify(s => s.DeleteCategoryAsync(1), Times.Once);
        }

        [Fact]
        public async Task DeleteCategory_ThrowsArgumentException_ReturnsNotFound()
        {
            _mockService.Setup(s => s.DeleteCategoryAsync(99)).ThrowsAsync(new ArgumentException("Category not found."));

            var result = await _controller.DeleteCategory(99);

            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Category not found.", notFound.Value);
        }
    }
}
