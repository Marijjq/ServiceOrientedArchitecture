using EventPlanner.Data;
using EventPlanner.Enums;
using EventPlanner.Models;
using EventPlanner.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EventPlanner.Repositories.Implementations
{
    public class InviteRepository : IInviteRepository
    {
        private readonly ApplicationDbContext _context;
        public InviteRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<Invite> GetInviteByIdAsync(int inviteId)
        {
            return await _context.Invites
                .Include(i => i.Inviter)
                .Include(i => i.Invitee)
                .Include(i => i.Event)
                .FirstOrDefaultAsync(i => i.Id == inviteId);
        }
        public async Task<IEnumerable<Invite>> GetAllInvitesAsync()
        {
            return await _context.Invites
                .Include(i => i.Event)
                .Include(i => i.Invitee)
                .ToListAsync();
        }
        public async Task AddInviteAsync(Invite invite)
        {
            await _context.Invites.AddAsync(invite);
            await _context.SaveChangesAsync();
        }
        public async Task UpdateInviteAsync(Invite invite)
        {
            _context.Invites.Update(invite);
            await _context.SaveChangesAsync();
        }
        public async Task DeleteInviteAsync(int inviteId)
        {
            var invite = await GetInviteByIdAsync(inviteId);
            if (invite != null)
            {
                _context.Invites.Remove(invite);
                await _context.SaveChangesAsync();
            }
        }
        //Additional
        public async Task<IEnumerable<Invite>> GetPendingInvitesByUserIdAsync(int userId)
        {
            return await _context.Invites
                .Where(i => i.InviteeId == userId && i.Status == InviteStatus.Pending)
                .ToListAsync();
        }

        public async Task<Invite?> GetByInviteeAndEventAsync(int inviteeId, int eventId)
        {
            return await _context.Invites
                .FirstOrDefaultAsync(i => i.InviteeId == inviteeId && i.EventId == eventId);
        }

    }
}
