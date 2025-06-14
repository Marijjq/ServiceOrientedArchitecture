using System.ComponentModel.DataAnnotations;

namespace EventPlanner.DTOs.Category
{
    public class CategoryCreateDTO
    {
        [Required, MaxLength(50)]
        public string Name { get; set; }

        [MaxLength(200)]
        public string? Description { get; set; }
    }
}
