using EventPlanner.Models;

namespace EventPlanner.Repositories.Interfaces
{
    public interface IRsvpRepository
    {
        Task<RSVP> GetRsvpByIdAsync(int rsvpId);
        Task<IEnumerable<RSVP>> GetAllRsvpsAsync();
        Task AddRsvpAsync(RSVP rsvp);
        Task UpdateRsvpAsync(RSVP rsvp);
        Task DeleteRsvpAsync(int rsvpId);

    }
}
