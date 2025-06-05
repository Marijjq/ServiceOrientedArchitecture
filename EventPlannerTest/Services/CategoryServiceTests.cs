using AutoMapper;
using EventPlanner.DTOs.Category;
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
    public class CategoryServiceTests
    {
        private readonly Mock<ICategoryRepository> _mockCategoryRepository;
        private readonly Mock<IMapper> _mockMapper;
        private readonly CategoryService _categoryService;

        public CategoryServiceTests()
        {
            _mockCategoryRepository = new Mock<ICategoryRepository>();
            _mockMapper = new Mock<IMapper>();

            _categoryService = new CategoryService(
                _mockCategoryRepository.Object,
                _mockMapper.Object
            );
        }

        [Fact]
        public async Task GetCategoryByIdAsync_CategoryExists_ReturnsCategoryDto()
        {
            var categoryId = 1;
            var category = new Category { Id = categoryId, Name = "Test" };
            var categoryDto = new CategoryDTO { Id = categoryId, Name = "Test" };

            _mockCategoryRepository.Setup(repo => repo.GetCategoryByIdAsync(categoryId))
                .ReturnsAsync(category);

            _mockMapper.Setup(mapper => mapper.Map<CategoryDTO>(category))
                .Returns(categoryDto);

            var result = await _categoryService.GetCategoryByIdAsync(categoryId);

            Assert.NotNull(result);
            Assert.Equal(categoryDto.Name, result.Name);
        }

        [Fact]
        public async Task GetAllCategoriesAsync_ReturnsMappedList()
        {
            var categories = new List<Category>
        {
            new Category { Id = 1, Name = "Cat 1" },
            new Category { Id = 2, Name = "Cat 2" }
        };

            var categoryDtos = new List<CategoryDTO>
        {
            new CategoryDTO { Id = 1, Name = "Cat 1" },
            new CategoryDTO { Id = 2, Name = "Cat 2" }
        };

            _mockCategoryRepository.Setup(repo => repo.GetAllCategoriesAsync())
                .ReturnsAsync(categories);

            _mockMapper.Setup(mapper => mapper.Map<IEnumerable<CategoryDTO>>(categories))
                .Returns(categoryDtos);

            var result = await _categoryService.GetAllCategoriesAsync();

            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task CreateCategoryAsync_NameAlreadyExists_ThrowsArgumentException()
        {
            var categoryCreateDto = new CategoryCreateDTO { Name = "Existing" };

            _mockCategoryRepository.Setup(repo => repo.GetCategoryByNameAsync(categoryCreateDto.Name))
                .ReturnsAsync(new Category { Name = "Existing" });

            await Assert.ThrowsAsync<ArgumentException>(() =>
                _categoryService.CreateCategoryAsync(categoryCreateDto));
        }

        [Fact]
        public async Task CreateCategoryAsync_ValidInput_ReturnsCategoryDto()
        {
            var categoryCreateDto = new CategoryCreateDTO { Name = "New Category" };
            var category = new Category { Id = 1, Name = "New Category" };
            var categoryDto = new CategoryDTO { Id = 1, Name = "New Category" };

            _mockCategoryRepository.Setup(repo => repo.GetCategoryByNameAsync(categoryCreateDto.Name))
                .ReturnsAsync((Category)null);

            _mockMapper.Setup(m => m.Map<Category>(categoryCreateDto)).Returns(category);
            _mockMapper.Setup(m => m.Map<CategoryDTO>(category)).Returns(categoryDto);

            var result = await _categoryService.CreateCategoryAsync(categoryCreateDto);

            _mockCategoryRepository.Verify(repo => repo.AddCategoryAsync(category), Times.Once);
            Assert.Equal(categoryDto.Name, result.Name);
        }

        [Fact]
        public async Task UpdateCategoryAsync_CategoryNotFound_ThrowsArgumentException()
        {
            var updateDto = new CategoryUpdateDTO { Name = "Updated Name" };

            _mockCategoryRepository.Setup(repo => repo.GetCategoryByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((Category)null);

            await Assert.ThrowsAsync<ArgumentException>(() =>
                _categoryService.UpdateCategoryAsync(1, updateDto));
        }

        [Fact]
        public async Task UpdateCategoryAsync_ValidInput_UpdatesAndReturnsDto()
        {
            var categoryId = 1;
            var existingCategory = new Category { Id = categoryId, Name = "Old" };
            var updateDto = new CategoryUpdateDTO { Name = "New" };
            var updatedDto = new CategoryDTO { Id = categoryId, Name = "New" };

            _mockCategoryRepository.Setup(repo => repo.GetCategoryByIdAsync(categoryId))
                .ReturnsAsync(existingCategory);

            _mockMapper.Setup(m => m.Map(updateDto, existingCategory)).Callback(() =>
            {
                existingCategory.Name = updateDto.Name;
            });

            _mockMapper.Setup(m => m.Map<CategoryDTO>(existingCategory)).Returns(updatedDto);

            var result = await _categoryService.UpdateCategoryAsync(categoryId, updateDto);

            _mockCategoryRepository.Verify(repo => repo.UpdateCategoryAsync(existingCategory), Times.Once);
            Assert.Equal("New", result.Name);
        }

        [Fact]
        public async Task DeleteCategoryAsync_CategoryNotFound_ThrowsArgumentException()
        {
            _mockCategoryRepository.Setup(repo => repo.GetCategoryByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((Category)null);

            await Assert.ThrowsAsync<ArgumentException>(() =>
                _categoryService.DeleteCategoryAsync(99));
        }

        [Fact]
        public async Task DeleteCategoryAsync_ValidCategory_CallsDelete()
        {
            var category = new Category { Id = 1, Name = "Delete Me" };

            _mockCategoryRepository.Setup(repo => repo.GetCategoryByIdAsync(1))
                .ReturnsAsync(category);

            await _categoryService.DeleteCategoryAsync(1);

            _mockCategoryRepository.Verify(repo => repo.DeleteCategoryAsync(1), Times.Once);
        }

        [Fact]
        public async Task GetCategoryByNameAsync_ReturnsMappedDto()
        {
            var name = "SearchCategory";
            var category = new Category { Id = 3, Name = name };
            var categoryDto = new CategoryDTO { Id = 3, Name = name };

            _mockCategoryRepository.Setup(repo => repo.GetCategoryByNameAsync(name))
                .ReturnsAsync(category);

            _mockMapper.Setup(m => m.Map<CategoryDTO>(category)).Returns(categoryDto);

            var result = await _categoryService.GetCategoryByNameAsync(name);

            Assert.Equal(name, result.Name);
        }
    }
}
