using AutoMapper;
using EventPlanner.DTOs.Category;
using EventPlanner.Models;

namespace EventPlanner.Profiles
{
    public class CategoryProfile : Profile 
    {
        public CategoryProfile()
        {
            CreateMap<Category, CategoryDTO>();
            CreateMap<CategoryCreateDTO, Category>();
            CreateMap<CategoryUpdateDTO, Category>();
        }
    }
    
}
