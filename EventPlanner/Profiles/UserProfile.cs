using AutoMapper;
using EventPlanner.DTOs.User;
using EventPlanner.Models;

namespace EventPlanner.Profiles
{
    public class UserProfile : Profile
    {
        public UserProfile() 
        {
            CreateMap<ApplicationUser, UserDTO>();
            CreateMap<UserCreateDTO, ApplicationUser>();
            CreateMap<UserUpdateDTO, ApplicationUser>();
        }
    }
}
