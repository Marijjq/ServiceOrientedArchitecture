using AutoMapper;
using EventPlanner.DTOs.Invite;
using EventPlanner.Models;

namespace EventPlanner.Profiles
{
    public class InviteProfile : Profile
    {
        public InviteProfile() 
        {
            CreateMap<Invite, InviteDTO>();
            CreateMap<InviteCreateDTO, Invite>();
            CreateMap<InviteUpdateDTO, Invite>();
        }
    }
}
