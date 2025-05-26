using AutoMapper;
using EventPlanner.DTOs.Category;
using EventPlanner.Models;
using EventPlanner.Repositories.Interfaces;
using EventPlanner.Services.Interfaces;

namespace EventPlanner.Services.Implementations
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;
        public CategoryService(ICategoryRepository categoryRepository, IMapper mapper)
        {
            _categoryRepository = categoryRepository;
            _mapper = mapper;
        }
        public async Task<CategoryDTO> GetCategoryByIdAsync(int categoryId)
        {
            var category = await _categoryRepository.GetCategoryByIdAsync(categoryId);
            return _mapper.Map<CategoryDTO>(category); 
        }
        public async Task<IEnumerable<CategoryDTO>> GetAllCategoriesAsync()
        {
            var categories = await _categoryRepository.GetAllCategoriesAsync();
            return _mapper.Map<IEnumerable<CategoryDTO>>(categories);
        }
        public async Task<CategoryDTO> CreateCategoryAsync(CategoryCreateDTO categoryDto)
        {
            var existingCategory = await _categoryRepository.GetCategoryByNameAsync(categoryDto.Name);
            if (existingCategory != null)
            {
                throw new ArgumentException("Category with this name already exists.");
            }


            var category = _mapper.Map<Category>(categoryDto);
            await _categoryRepository.AddCategoryAsync(category);
            return _mapper.Map<CategoryDTO>(category);
        }

        public async Task<CategoryDTO> UpdateCategoryAsync(int categoryId, CategoryUpdateDTO categoryDto)
        {
            var existingCategory = await _categoryRepository.GetCategoryByIdAsync(categoryId);
            if (existingCategory == null)
            {
                throw new ArgumentException("Category not found.");
            }

            _mapper.Map(categoryDto, existingCategory); // Applies updates
            existingCategory.UpdatedAt = DateTime.UtcNow;

            await _categoryRepository.UpdateCategoryAsync(existingCategory);
            return _mapper.Map<CategoryDTO>(existingCategory);
        }
        public async Task DeleteCategoryAsync(int categoryId)
        {
            var category = await _categoryRepository.GetCategoryByIdAsync(categoryId);
            if (category == null)
            {
                throw new ArgumentException("Category not found.");
            }

            await _categoryRepository.DeleteCategoryAsync(categoryId);
        }

        // Additional
        public async Task<CategoryDTO> GetCategoryByNameAsync(string name)
        {
            var category = await _categoryRepository.GetCategoryByNameAsync(name);
            return _mapper.Map<CategoryDTO>(category);
        }
    }

}
