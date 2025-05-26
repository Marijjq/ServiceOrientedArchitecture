using EventPlanner.Models;

namespace EventPlanner.Repositories.Interfaces
{
    public interface ICategoryRepository
    {
        Task<Category> GetCategoryByIdAsync(int id);
        Task<IEnumerable<Category>> GetAllCategoriesAsync();
        Task AddCategoryAsync(Category category);
        Task UpdateCategoryAsync(Category category);
        Task DeleteCategoryAsync(int id);

        //Additional
        Task<Category> GetCategoryByNameAsync(string name);
    }
}
