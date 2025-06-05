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
        }

        [Fact]
        public async Task GetAllCategories_ReturnsOkWithList()
        {
            var mockCategories = new List<CategoryDTO> { new CategoryDTO { Id = 1 }, new CategoryDTO { Id = 2 } };
            _mockService.Setup(s => s.GetAllCategoriesAsync()).ReturnsAsync(mockCategories);

            var result = await _controller.GetAllCategories();

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var categories = Assert.IsAssignableFrom<IEnumerable<CategoryDTO>>(okResult.Value);
            Assert.Equal(2, ((List<CategoryDTO>)categories).Count);
        }

        [Fact]
        public async Task GetCategoryById_ReturnsOk_WhenFound()
        {
            var category = new CategoryDTO { Id = 1 };
            _mockService.Setup(s => s.GetCategoryByIdAsync(1)).ReturnsAsync(category);

            var result = await _controller.GetCategoryById(1);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedCategory = Assert.IsType<CategoryDTO>(okResult.Value);
            Assert.Equal(1, returnedCategory.Id);
        }

        [Fact]
        public async Task GetCategoryById_ReturnsNotFound_WhenMissing()
        {
            _mockService.Setup(s => s.GetCategoryByIdAsync(10)).ReturnsAsync((CategoryDTO)null);

            var result = await _controller.GetCategoryById(10);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task CreateCategory_ReturnsCreatedAtAction_WhenSuccessful()
        {
            var createDto = new CategoryCreateDTO { Name = "New" };
            var createdDto = new CategoryDTO { Id = 5, Name = "New" };
            _mockService.Setup(s => s.CreateCategoryAsync(createDto)).ReturnsAsync(createdDto);

            var result = await _controller.CreateCategory(createDto);

            var created = Assert.IsType<CreatedAtActionResult>(result.Result);
            var returned = Assert.IsType<CategoryDTO>(created.Value);
            Assert.Equal("New", returned.Name);
            Assert.Equal(5, created.RouteValues["id"]);
        }

        [Fact]
        public async Task CreateCategory_ReturnsBadRequest_WhenNullInput()
        {
            var result = await _controller.CreateCategory(null);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("Category cannot be null.", badRequest.Value);
        }

        [Fact]
        public async Task CreateCategory_ReturnsBadRequest_OnArgumentException()
        {
            var createDto = new CategoryCreateDTO { Name = "Invalid" };
            _mockService.Setup(s => s.CreateCategoryAsync(createDto)).ThrowsAsync(new ArgumentException("Invalid name"));

            var result = await _controller.CreateCategory(createDto);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("Invalid name", badRequest.Value);
        }

        [Fact]
        public async Task UpdateCategory_ReturnsNoContent_WhenSuccessful()
        {
            var updateDto = new CategoryUpdateDTO { Name = "Updated" };
            _mockService.Setup(s => s.UpdateCategoryAsync(3, updateDto)).ReturnsAsync(new CategoryDTO { Id = 3 });

            var result = await _controller.UpdateCategory(3, updateDto);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task UpdateCategory_ReturnsNotFound_WhenNotFound()
        {
            var updateDto = new CategoryUpdateDTO { Name = "Updated" };
            _mockService.Setup(s => s.UpdateCategoryAsync(3, updateDto)).ReturnsAsync((CategoryDTO)null);

            var result = await _controller.UpdateCategory(3, updateDto);

            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Category not found.", notFound.Value);
        }

        [Fact]
        public async Task UpdateCategory_ReturnsNotFound_OnArgumentException()
        {
            var updateDto = new CategoryUpdateDTO { Name = "Invalid" };
            _mockService.Setup(s => s.UpdateCategoryAsync(4, updateDto)).ThrowsAsync(new ArgumentException("Category does not exist"));

            var result = await _controller.UpdateCategory(4, updateDto);

            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Category does not exist", notFound.Value);
        }

        [Fact]
        public async Task DeleteCategory_ReturnsNoContent_WhenSuccessful()
        {
            _mockService.Setup(s => s.DeleteCategoryAsync(3)).Returns(Task.CompletedTask);

            var result = await _controller.DeleteCategory(3);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteCategory_ReturnsNotFound_OnArgumentException()
        {
            _mockService.Setup(s => s.DeleteCategoryAsync(99)).ThrowsAsync(new ArgumentException("Category not found"));

            var result = await _controller.DeleteCategory(99);

            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Category not found", notFound.Value);
        }

        [Fact]
        public async Task GetCategoryByName_ReturnsOk_WhenFound()
        {
            var category = new CategoryDTO { Id = 1, Name = "Conference" };
            _mockService.Setup(s => s.GetCategoryByNameAsync("Conference")).ReturnsAsync(category);

            var result = await _controller.GetCategoryByName("Conference");

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returned = Assert.IsType<CategoryDTO>(okResult.Value);
            Assert.Equal("Conference", returned.Name);
        }

        [Fact]
        public async Task GetCategoryByName_ReturnsNotFound_WhenMissing()
        {
            _mockService.Setup(s => s.GetCategoryByNameAsync("Missing")).ReturnsAsync((CategoryDTO)null);

            var result = await _controller.GetCategoryByName("Missing");

            Assert.IsType<NotFoundResult>(result.Result);
        }


    }
}
