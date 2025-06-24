using EventPlanner.Data;
using EventPlanner.Models;
using EventPlanner.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EventPlanner.Repositories.Implementations
{
    public class RsvpRepository : IRsvpRepository
    {
        private readonly ApplicationDbContext _context;
        public RsvpRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<RSVP?> GetRsvpByIdAsync(int id)
        {
            return await _context.RSVPs
                .Include(r => r.Event)
                .Include(r => r.User)
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<IEnumerable<RSVP>> GetAllRsvpsAsync()
        {
            return await _context.RSVPs
                .AsNoTracking() 
                .ToListAsync();
        }
        public async Task AddRsvpAsync(RSVP rsvp)
        {
            await _context.RSVPs.AddAsync(rsvp);
            await _context.SaveChangesAsync();
        }
        public async Task UpdateRsvpAsync(RSVP rsvp)
        {
            _context.RSVPs.Update(rsvp);
            await _context.SaveChangesAsync();
        }
        public async Task DeleteRsvpAsync(int rsvpId)
        {
            var rsvp = await GetRsvpByIdAsync(rsvpId);
            if (rsvp != null)
            {
                _context.RSVPs.Remove(rsvp);
                await _context.SaveChangesAsync();
            }
        }

        // Additional 
        public async Task<IEnumerable<RSVP>> GetRsvpByEventIdAsync(int eventId)
        {
            return await _context.RSVPs
                .Include(r => r.Event)
                .Include(r => r.User)
                .Where(r => r.EventId == eventId)
                .AsNoTracking()
                .ToListAsync();
        }


        public async Task<IEnumerable<RSVP>> GetRsvpsByUserIdAsync(string userId)
        {
            return await _context.RSVPs
                .Where(r => r.UserId == userId)
                .AsNoTracking()
                .ToListAsync();
        }


    }
}
