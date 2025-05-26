using EventPlanner.DTOs.Category;
using EventPlanner.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventPlanner.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,User")]

        public async Task<ActionResult<IEnumerable<CategoryDTO>>> GetAllCategories()
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            return Ok(categories);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,User")]

        public async Task<ActionResult<CategoryDTO>> GetCategoryById(int id)
        {
            var category = await _categoryService.GetCategoryByIdAsync(id);
            if (category == null)
                return NotFound();
            return Ok(category);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]

        public async Task<ActionResult<CategoryDTO>> CreateCategory([FromBody] CategoryCreateDTO category)
        {
            if (category == null)
                return BadRequest("Category cannot be null.");
            try
            {
                var createdCategory = await _categoryService.CreateCategoryAsync(category);
                return CreatedAtAction(nameof(GetCategoryById), new { id = createdCategory.Id }, createdCategory);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> UpdateCategory(int id, [FromBody] CategoryUpdateDTO category)
        {
            try
            {
                var updated = await _categoryService.UpdateCategoryAsync(id, category);
                if (updated == null)
                    return NotFound("Category not found.");
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> DeleteCategory(int id)
        {
            try
            {
                await _categoryService.DeleteCategoryAsync(id);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet("name/{name}")]
        [Authorize(Roles = "Admin,User")]

        public async Task<ActionResult<CategoryDTO>> GetCategoryByName(string name)
        {
            var category = await _categoryService.GetCategoryByNameAsync(name);
            if (category == null)
                return NotFound();
            return Ok(category);
        }
    }
}
