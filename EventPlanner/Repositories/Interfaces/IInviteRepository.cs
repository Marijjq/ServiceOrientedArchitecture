﻿using EventPlanner.Enums;
using EventPlanner.Models;

namespace EventPlanner.Repositories.Interfaces
{
    public interface IInviteRepository
    {
        Task<Invite> GetInviteByIdAsync(int inviteId);
        Task<IEnumerable<Invite>> GetAllInvitesAsync();
        Task AddInviteAsync(Invite invite);
        Task UpdateInviteAsync(Invite invite);
        Task DeleteInviteAsync(int inviteId);

        //Additional
        Task<IEnumerable<Invite>> GetPendingInvitesByUserIdAsync(string userId);
        Task<Invite?> GetByInviteeAndEventAsync(string inviteeId, int eventId);


    }
}
