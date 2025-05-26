using AutoMapper;
using EventPlanner.DTOs.User;
using EventPlanner.Models;

namespace EventPlanner.Profiles
{
    public class UserProfile : Profile
    {
        public UserProfile() 
        {
            CreateMap<User, UserDTO>();
            CreateMap<UserCreateDTO, User>();
            CreateMap<UserUpdateDTO, User>();
        }
    }
}
