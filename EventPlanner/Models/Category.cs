using System.ComponentModel.DataAnnotations;

namespace EventPlanner.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(50)]
        public string Name { get; set; }

        [MaxLength(200)]
        public string Description { get; set; }
        public List<Event> Events { get; set; } = new();
    }
}
