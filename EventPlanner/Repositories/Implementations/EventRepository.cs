using EventPlanner.Data;
using EventPlanner.Enums;
using EventPlanner.Models;
using EventPlanner.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EventPlanner.Repositories.Implementations
{
    public class EventRepository : IEventRepository
    {
        private readonly ApplicationDbContext _context;
        public EventRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<Event> GetEventByIdAsync(int id)
        {
            return await _context.Events.FindAsync(id);
        }
        public async Task<IEnumerable<Event>> GetAllEventsAsync()
        {
            return await _context.Events.ToListAsync();
        }
        public async Task AddEventAsync(Event eventItem)
        {
            await _context.Events.AddAsync(eventItem);
            await _context.SaveChangesAsync();
        }
        public async Task UpdateEventAsync(Event eventItem)
        {
            _context.Events.Update(eventItem);
            await _context.SaveChangesAsync();
        }
        public async Task DeleteEventAsync(int id)
        {
            var eventItem = await GetEventByIdAsync(id);
            if (eventItem != null)
            {
                _context.Events.Remove(eventItem);
                await _context.SaveChangesAsync();
            }
        }

        // Additional methods
        public async Task<IEnumerable<Event>> GetEventsByUserIdAsync(int userId)
        {
            return await _context.Events
                .Where(e => e.UserId == userId)
                .ToListAsync();
        }
        public async Task<IEnumerable<Event>> GetEventsByCategoryIdAsync(int categoryId)
        {
            return await _context.Events
                .Where(e => e.CategoryId == categoryId)
                .ToListAsync();
        }
    
    public async Task<IEnumerable<Event>> GetEventsByStatusAsync(EventStatus status)
        {
            return await _context.Events
                .Where(e => e.Status == status)
                .ToListAsync();
        }
    }
}
