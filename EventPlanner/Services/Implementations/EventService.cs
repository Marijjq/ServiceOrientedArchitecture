using AutoMapper;
using EventPlanner.DTOs.Event;
using EventPlanner.Enums;
using EventPlanner.Models;
using EventPlanner.Repositories.Interfaces;
using EventPlanner.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace EventPlanner.Services.Implementations
{
    public class EventService : IEventService
    {
        private readonly IEventRepository _eventRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;
        private readonly IMemoryCache _cache;

        public EventService(IEventRepository eventRepository, 
            ICategoryRepository categoryRepository, IMapper mapper, IMemoryCache memoryCache)
        {
            _eventRepository = eventRepository;
            _categoryRepository = categoryRepository;
            _mapper = mapper;
            _cache = memoryCache;
        }

        public async Task<EventDTO> GetEventByIdAsync(int id)
        {
            var eventItem = await _eventRepository.GetEventByIdAsync(id);
            return eventItem == null ? null : _mapper.Map<EventDTO>(eventItem);
        }

        public async Task<IEnumerable<EventDTO>> GetAllEventsAsync()
        {
            if (!_cache.TryGetValue("events", out IEnumerable<EventDTO> cached))
            {
                var events = await _eventRepository.GetAllEventsAsync();
                cached = _mapper.Map<IEnumerable<EventDTO>>(events);

                _cache.Set("events", cached, new MemoryCacheEntryOptions
                {
                    SlidingExpiration = TimeSpan.FromMinutes(10)
                });
            }

            return cached;
        }


        public async Task<EventDTO> CreateEventAsync(EventCreateDTO eventDto, string userId)
        {
            // Basic validation
            if (string.IsNullOrWhiteSpace(eventDto.Title) || string.IsNullOrWhiteSpace(eventDto.Location))
                throw new ArgumentException("Title and location cannot be empty.");

            if (eventDto.StartDate < DateTime.Now)
                throw new ArgumentException("Start date cannot be in the past.");

            if (eventDto.StartDate >= eventDto.EndDate)
                throw new ArgumentException("End date must be after start date.");

            if (eventDto.MaxParticipants <= 0)
                throw new ArgumentException("Max participants must be greater than 0.");

            if (!Enum.IsDefined(typeof(EventStatus), eventDto.Status))
                eventDto.Status = EventStatus.Upcoming;

            var category = await _categoryRepository.GetCategoryByIdAsync(eventDto.CategoryId);
            if (category == null)
                throw new ArgumentException("Category does not exist.");

            var eventEntity = _mapper.Map<Event>(eventDto);
            eventEntity.UserId = userId;
            eventEntity.IsPrivate = eventDto.IsPrivate;

            await _eventRepository.AddEventAsync(eventEntity);
            _cache.Remove("events");
            return _mapper.Map<EventDTO>(eventEntity);
        }

        public async Task<EventDTO> UpdateEventAsync(int id, EventUpdateDTO eventDto, string userId)
        {
            var existingEvent = await _eventRepository.GetEventByIdAsync(id);
            if (existingEvent == null)
                throw new ArgumentException("Event not found.");

            if (existingEvent.UserId != userId)
                throw new UnauthorizedAccessException("You are not authorized to update this event.");

            if (string.IsNullOrWhiteSpace(eventDto.Title) || string.IsNullOrWhiteSpace(eventDto.Location))
                throw new ArgumentException("Title and location cannot be empty.");

            if (eventDto.StartDate < DateTime.Now)
                throw new ArgumentException("Start date cannot be in the past.");

            if (eventDto.StartDate >= eventDto.EndDate)
                throw new ArgumentException("End date must be after start date.");

            if (eventDto.MaxParticipants <= 0)
                throw new ArgumentException("Max participants must be greater than 0.");

            var category = await _categoryRepository.GetCategoryByIdAsync(eventDto.CategoryId);
            if (category == null)
                throw new ArgumentException("Category does not exist.");

            // Apply changes
            existingEvent.Title = eventDto.Title;
            existingEvent.Description = eventDto.Description;
            existingEvent.Location = eventDto.Location;
            existingEvent.StartDate = eventDto.StartDate;
            existingEvent.EndDate = eventDto.EndDate;
            existingEvent.MaxParticipants = eventDto.MaxParticipants;
            existingEvent.Status = eventDto.Status;
            existingEvent.CategoryId = eventDto.CategoryId;
            existingEvent.IsPrivate = eventDto.IsPrivate;

            await _eventRepository.UpdateEventAsync(existingEvent);
            _cache.Remove("events");

            return _mapper.Map<EventDTO>(existingEvent);
        }

        public async Task DeleteEventAsync(int id)
        {
            var eventItem = await _eventRepository.GetEventByIdAsync(id);
            if (eventItem == null)
                throw new ArgumentException("Event not found.");

            if (eventItem.Status == EventStatus.Completed)
                throw new InvalidOperationException("Cannot delete a completed event.");

            _cache.Remove("events");

            await _eventRepository.DeleteEventAsync(id);
        }

        public async Task<IEnumerable<EventDTO>> GetEventsByUserIdAsync(string userId)
        {
            var events = await _eventRepository.GetEventsByUserIdAsync(userId);
            return _mapper.Map<IEnumerable<EventDTO>>(events);
        }

        public async Task<IEnumerable<EventDTO>> GetEventsByCategoryIdAsync(int categoryId)
        {
            var events = await _eventRepository.GetEventsByCategoryIdAsync(categoryId);
            return _mapper.Map<IEnumerable<EventDTO>>(events);
        }

        public async Task<IEnumerable<EventDTO>> GetEventsByStatusAsync(EventStatus status)
        {
            var events = await _eventRepository.GetEventsByStatusAsync(status);
            return _mapper.Map<IEnumerable<EventDTO>>(events);
        }

        public async Task<bool> IsEventFullAsync(int eventId, int currentParticipants)
        {
            var eventItem = await _eventRepository.GetEventByIdAsync(eventId);
            return eventItem?.MaxParticipants <= currentParticipants;
        }

        public async Task<bool> IsEventUpcomingAsync(int eventId)
        {
            var eventItem = await _eventRepository.GetEventByIdAsync(eventId);
            return eventItem != null &&
                   eventItem.Status == EventStatus.Upcoming &&
                   eventItem.StartDate > DateTime.UtcNow;
        }

        public async Task<int> GetRemainingSpotsAsync(int eventId, int currentParticipants)
        {
            var eventItem = await _eventRepository.GetEventByIdAsync(eventId);
            return eventItem?.MaxParticipants - currentParticipants ?? 0;
        }

        public async Task<IEnumerable<EventDTO>> GetUpcomingEventsAsync()
        {
            var events = await _eventRepository.GetAllEventsAsync();
            return _mapper.Map<IEnumerable<EventDTO>>(events
                .Where(e => e.Status == EventStatus.Upcoming && e.StartDate > DateTime.UtcNow));
        }

        public async Task<IEnumerable<EventDTO>> SearchEventsByTitleAsync(string keyword)
        {
            var events = await _eventRepository.GetAllEventsAsync();
            return _mapper.Map<IEnumerable<EventDTO>>(events
                .Where(e => e.Title.Contains(keyword, StringComparison.OrdinalIgnoreCase)));
        }

        public async Task ToggleEventStatusAsync(int id, EventStatus status, string userId)
        {
            var eventItem = await _eventRepository.GetEventByIdAsync(id);
            if (eventItem == null)
                throw new ArgumentException("Event not found.");

            if (eventItem.UserId != userId)
                throw new UnauthorizedAccessException("You are not authorized to change this event's status.");

            eventItem.Status = status;
            await _eventRepository.UpdateEventAsync(eventItem);
        }
    }
}
