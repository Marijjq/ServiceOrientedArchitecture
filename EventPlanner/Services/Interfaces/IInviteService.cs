using EventPlanner.DTOs.Invite;
using EventPlanner.Models;

namespace EventPlanner.Services.Interfaces
{
    public interface IInviteService
    {
        Task<InviteDTO> GetInviteByIdAsync(int id);
        Task<IEnumerable<InviteDTO>> GetAllInvitesAsync();
        Task<InviteDTO> UpdateInviteAsync(int id, InviteUpdateDTO inviteDto);
        Task DeleteInviteAsync(int id);
        Task<IEnumerable<InviteDTO>> GetPendingInvitesByUserIdAsync(string userId);
        Task<InviteDTO?> GetByInviteeAndEventAsync(string inviteeId, int eventId);

        // Unified Invite Creation Method
        Task<InviteDTO> SendInviteAsync(InviteCreateDTO inviteDto);


    }
}
