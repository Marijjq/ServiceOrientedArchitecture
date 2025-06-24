using AutoMapper;
using EventPlanner.DTOs.Invite;
using EventPlanner.Models;

namespace EventPlanner.Profiles
{
    public class InviteProfile : Profile
    {
        public InviteProfile()
        {
            CreateMap<Invite, InviteDTO>()
                .ForMember(dest => dest.InviterName,
                    opt => opt.MapFrom(src => src.Inviter != null ? $"{src.Inviter.FirstName} {src.Inviter.LastName}" : "N/A"))
                .ForMember(dest => dest.InviteeName,
                    opt => opt.MapFrom(src => src.Invitee != null ? $"{src.Invitee.FirstName} {src.Invitee.LastName}" : "N/A"))
                .ForMember(dest => dest.EventTitle,
                    opt => opt.MapFrom(src => src.Event != null ? src.Event.Title : "N/A"));

            CreateMap<InviteCreateDTO, Invite>();
            CreateMap<InviteUpdateDTO, Invite>();
        }
    }
}
