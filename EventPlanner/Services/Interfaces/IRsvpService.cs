using EventPlanner.DTOs.RSVP;
using EventPlanner.Enums;
using EventPlanner.Models;

namespace EventPlanner.Services.Interfaces
{
    public interface IRsvpService
    {
        Task<RsvpDTO> GetRsvpByIdAsync(int id);
        Task<IEnumerable<RsvpDTO>> GetAllRsvpsAsync();
        Task<RsvpDTO> CreateRsvpAsync(RsvpCreateDTO rsvpDto);
        Task<RsvpDTO> UpdateRsvpAsync(int id, RsvpUpdateDTO rsvpDto);
        Task DeleteRsvpAsync(int id);

        //Additional
        Task<IEnumerable<RsvpDTO>> GetRsvpsByEventIdAsync(int eventId);
        Task<IEnumerable<RsvpDTO>> GetRsvpsByUserIdAsync(int userId);
        Task<RsvpDTO> RespondToInviteAsync(int inviteId, RSVPStatus responseStatus);
        Task<bool> UpdateStatusAsync(int id, RSVPStatus status); // "Attending", "NotAttending", "Cancelled"
        Task<RsvpDTO> CancelRsvpAsync(int id); // Explicit method for cancellation
    }
}
