using AutoMapper;
using EventPlanner.DTOs.RSVP;
using EventPlanner.Models;

namespace EventPlanner.Profiles
{
    public class RsvpProfile : Profile
    {
        public RsvpProfile() 
        {
            CreateMap<RSVP, RsvpDTO>();
            CreateMap<RsvpCreateDTO, RSVP>();
            CreateMap<RsvpUpdateDTO, RSVP   >();
        }
    }
}
