using AutoMapper;
using EventPlanner.DTOs.Category;
using EventPlanner.DTOs.Event;
using EventPlanner.Models;

namespace EventPlanner.Profiles
{
    public class EventProfile : Profile
    {
        public EventProfile() 
        {
            CreateMap<Event, EventDTO>();
            CreateMap<EventCreateDTO, Event>();
            CreateMap<EventUpdateDTO, Event>();

        }
    }
}
