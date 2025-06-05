using EventPlanner.Enums;
using EventPlanner.Models;

namespace EventPlanner.Repositories.Interfaces
{
    public interface IEventRepository
    {
        Task<Event> GetEventByIdAsync(int id);
        Task<IEnumerable<Event>> GetAllEventsAsync();
        Task AddEventAsync(Event eventItem);
        Task UpdateEventAsync(Event eventItem);
        Task DeleteEventAsync(int id);

        //Additional
        Task<IEnumerable<Event>> GetEventsByUserIdAsync(string userId);
        Task<IEnumerable<Event>> GetEventsByCategoryIdAsync(int categoryId);
        Task<IEnumerable<Event>> GetEventsByStatusAsync(EventStatus status);

    }
}
