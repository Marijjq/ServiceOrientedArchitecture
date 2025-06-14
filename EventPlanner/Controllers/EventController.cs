using EventPlanner.DTOs.Event;
using EventPlanner.Enums;
using EventPlanner.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventPlanner.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class EventController : ControllerBase
    {
        private readonly IEventService _eventService;

        public EventController(IEventService eventService)
        {
            _eventService = eventService;
        }

        [HttpGet]
        [AllowAnonymous]
        [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Client)]
        public async Task<ActionResult<IEnumerable<EventDTO>>> GetAllEvents()
        {
            var events = await _eventService.GetAllEventsAsync();
            return Ok(await _eventService.GetAllEventsAsync());
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<EventDTO>> GetEventById(int id)
        {
            var eventItem = await _eventService.GetEventByIdAsync(id);
            if (eventItem == null)
                return NotFound("Event not found.");
            return Ok(eventItem);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Organizer")]
        public async Task<ActionResult<EventDTO>> CreateEvent([FromBody] EventCreateDTO eventItem)
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized("User ID not found.");
            }
            string userId = userIdClaim.Value;

            try
            {
                var createdEvent = await _eventService.CreateEventAsync(eventItem, userId);
                return CreatedAtAction(nameof(GetEventById), new { id = createdEvent.Id }, createdEvent);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Organizer")]
        public async Task<ActionResult<EventDTO>> UpdateEvent(int id, [FromBody] EventUpdateDTO eventItem)
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized("User ID not found.");
            }
            string userId = userIdClaim.Value;

            try
            {
                var updatedEvent = await _eventService.UpdateEventAsync(id, eventItem, userId);
                return Ok(updatedEvent);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid("You are not authorized to update this event.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            try
            {
                await _eventService.DeleteEventAsync(id);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("user/{userId}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<EventDTO>>> GetEventsByUserId(string userId)
        {
            var events = await _eventService.GetEventsByUserIdAsync(userId);
            return Ok(events);
        }

        [HttpGet("category/{categoryId}")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<EventDTO>>> GetEventsByCategoryId(int categoryId)
        {
            var events = await _eventService.GetEventsByCategoryIdAsync(categoryId);
            return Ok(events);
        }

        [HttpGet("status/{status}")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<EventDTO>>> GetEventsByStatus(EventStatus status)
        {
            var events = await _eventService.GetEventsByStatusAsync(status);
            return Ok(events);
        }

        [HttpPut("toggle-status/{id}/{status}")]
        [Authorize(Roles = "Admin,Organizer")]
        public async Task<IActionResult> ToggleEventStatus(int id, EventStatus status)
        {
            var userIdClaim = User.FindFirst("id");
            if (userIdClaim == null)
            {
                return Unauthorized("User ID not found.");
            }
            string userId = userIdClaim.Value;

            try
            {
                await _eventService.ToggleEventStatusAsync(id, status, userId);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("upcoming")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<EventDTO>>> GetUpcomingEvents()
        {
            var upcomingEvents = await _eventService.GetUpcomingEventsAsync();
            return Ok(upcomingEvents);
        }

        [HttpGet("search/{keyword}")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<EventDTO>>> SearchEventsByTitle(string keyword)
        {
            var events = await _eventService.SearchEventsByTitleAsync(keyword);
            return Ok(events);
        }
        [HttpGet("{id}/is-full/{currentParticipants}")]
        [Authorize]
        public async Task<ActionResult<bool>> IsEventFull(int id, int currentParticipants)
        {
            var isFull = await _eventService.IsEventFullAsync(id, currentParticipants);
            return Ok(isFull);
        }

        [HttpGet("{id}/is-upcoming")]
        [Authorize]
        public async Task<ActionResult<bool>> IsEventUpcoming(int id)
        {
            var isUpcoming = await _eventService.IsEventUpcomingAsync(id);
            return Ok(isUpcoming);
        }

        [HttpGet("{id}/remaining-spots/{currentParticipants}")]
        [Authorize]
        public async Task<ActionResult<int>> GetRemainingSpots(int id, int currentParticipants)
        {
            var remainingSpots = await _eventService.GetRemainingSpotsAsync(id, currentParticipants);
            return Ok(remainingSpots);
        }

    }
}
