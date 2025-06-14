using AutoMapper;
using EventPlanner.DTOs.Category;
using EventPlanner.Models;
using EventPlanner.Repositories.Interfaces;
using EventPlanner.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace EventPlanner.Services.Implementations
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;
        private readonly IMemoryCache _cache;
        public CategoryService(ICategoryRepository categoryRepository, IMapper mapper, IMemoryCache memoryCache)
        {
            _categoryRepository = categoryRepository;
            _mapper = mapper;
            _cache = memoryCache;
        }
        public async Task<CategoryDTO> GetCategoryByIdAsync(int categoryId)
        {
            var category = await _categoryRepository.GetCategoryByIdAsync(categoryId);
            return _mapper.Map<CategoryDTO>(category); 
        }
        public async Task<IEnumerable<CategoryDTO>> GetAllCategoriesAsync()
        {
            if (!_cache.TryGetValue("categories", out IEnumerable<CategoryDTO> cached))
            {
                var categories = await _categoryRepository.GetAllCategoriesAsync();
                cached = _mapper.Map<IEnumerable<CategoryDTO>>(categories);

                _cache.Set("categories", cached, new MemoryCacheEntryOptions
                {
                    SlidingExpiration = TimeSpan.FromMinutes(10)
                });
            }

            return cached;
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
            _cache.Remove("categories");

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
            _cache.Remove("categories");

            return _mapper.Map<CategoryDTO>(existingCategory);
        }
        public async Task DeleteCategoryAsync(int categoryId)
        {
            var category = await _categoryRepository.GetCategoryByIdAsync(categoryId);
            if (category == null)
            {
                throw new ArgumentException("Category not found.");
            }
            _cache.Remove("categories");

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
