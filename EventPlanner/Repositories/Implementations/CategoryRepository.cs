using EventPlanner.Data;
using EventPlanner.Models;
using EventPlanner.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EventPlanner.Repositories.Implementations
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly ApplicationDbContext _context;
        public CategoryRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<Category> GetCategoryByIdAsync(int id)
        {
            return await _context.Categories
                .AsNoTracking() 
                .FirstOrDefaultAsync(c => c.Id == id);

        }
        public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
        {
            return await _context.Categories
                .AsNoTracking()
                .ToListAsync();
        }
        public async Task AddCategoryAsync(Category category)
        { 
            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();
        }
        public async Task UpdateCategoryAsync(Category category)
        {
            _context.Categories.Update(category);
            await _context.SaveChangesAsync();

        }
        public async Task DeleteCategoryAsync(int id)
        {
            var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);
            if (category != null)
            {
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
            }
        }

        // Additional
        public async Task<Category> GetCategoryByNameAsync(string name)
        {
                return await _context.Categories
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Name == name);
        }
    }
}
