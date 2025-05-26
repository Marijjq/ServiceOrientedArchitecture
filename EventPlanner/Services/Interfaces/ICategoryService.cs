using EventPlanner.DTOs.Category;
using EventPlanner.Models;

namespace EventPlanner.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<CategoryDTO> GetCategoryByIdAsync(int categoryId);
        Task<IEnumerable<CategoryDTO>> GetAllCategoriesAsync();
        Task<CategoryDTO> CreateCategoryAsync(CategoryCreateDTO categoryDto);
        Task<CategoryDTO> UpdateCategoryAsync(int categoryId, CategoryUpdateDTO categoryDto);
        Task DeleteCategoryAsync(int categoryId);

        //Additional
        Task<CategoryDTO> GetCategoryByNameAsync(string name);
    }
}
