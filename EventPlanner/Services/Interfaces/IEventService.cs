using EventPlanner.DTOs.Event;
using EventPlanner.Enums;
using EventPlanner.Models;

namespace EventPlanner.Services.Interfaces
{
    public interface IEventService
    {
        Task<EventDTO> GetEventByIdAsync(int id);
        Task<IEnumerable<EventDTO>> GetAllEventsAsync();
        Task<EventDTO> CreateEventAsync(EventCreateDTO eventItemDto, int userId);
        Task<EventDTO> UpdateEventAsync(int id, EventUpdateDTO eventItemDto, int userId);
        Task DeleteEventAsync(int id );

        //Additional
        Task<IEnumerable<EventDTO>> GetEventsByUserIdAsync(int userId);
        Task<IEnumerable<EventDTO>> GetEventsByCategoryIdAsync(int categoryId);
        Task<IEnumerable<EventDTO>> GetEventsByStatusAsync(EventStatus status);
        Task<bool> IsEventFullAsync(int id, int participants);
        Task<bool> IsEventUpcomingAsync(int id);
        Task<int> GetRemainingSpotsAsync(int id, int participants);
        Task<IEnumerable<EventDTO>> GetUpcomingEventsAsync();
        Task<IEnumerable<EventDTO>> SearchEventsByTitleAsync(string keyword);
        Task ToggleEventStatusAsync(int id, EventStatus status, int userId);



    }
}
