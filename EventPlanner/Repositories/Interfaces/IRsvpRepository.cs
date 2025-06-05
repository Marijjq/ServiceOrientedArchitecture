using EventPlanner.Models;

namespace EventPlanner.Repositories.Interfaces
{
    public interface IRsvpRepository
    {
        Task<RSVP> GetRsvpByIdAsync(int id);
        Task<IEnumerable<RSVP>> GetAllRsvpsAsync();
        Task AddRsvpAsync(RSVP rsvp);
        Task UpdateRsvpAsync(RSVP rsvp);
        Task DeleteRsvpAsync(int id);

        //Additional
        Task<IEnumerable<RSVP>> GetRsvpByEventIdAsync(int eventId);
        Task<IEnumerable<RSVP>> GetRsvpsByUserIdAsync(string userId);
    }
}
