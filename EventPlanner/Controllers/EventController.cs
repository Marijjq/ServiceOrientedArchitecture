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

        // GET: api/event
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<EventDTO>>> GetAllEvents()
        {
            var events = await _eventService.GetAllEventsAsync();
            return Ok(events);
        }

        // GET: api/event/{id}
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<EventDTO>> GetEventById(int id)
        {
            var eventItem = await _eventService.GetEventByIdAsync(id);
            if (eventItem == null)
                return NotFound("Event not found.");
            return Ok(eventItem);
        }

        // POST: api/event
        [HttpPost]
        [Authorize(Roles = "Admin,Organizer")]
        public async Task<ActionResult<EventDTO>> CreateEvent([FromBody] EventCreateDTO eventItem)
        {
            var userIdClaim = User.FindFirst("id");
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


        // PUT: api/event/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Organizer")]
        public async Task<ActionResult<EventDTO>> UpdateEvent(int id, [FromBody] EventUpdateDTO eventItem)
        {
            var userIdClaim = User.FindFirst("id");
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


        // DELETE: api/event/{id}
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

        // GET: api/event/user/{userId}
        [HttpGet("user/{userId}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<EventDTO>>> GetEventsByUserId(string userId)
        {
            var events = await _eventService.GetEventsByUserIdAsync(userId);
            return Ok(events);
        }

        // GET: api/event/category/{categoryId}
        [HttpGet("category/{categoryId}")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<EventDTO>>> GetEventsByCategoryId(int categoryId)
        {
            var events = await _eventService.GetEventsByCategoryIdAsync(categoryId);
            return Ok(events);
        }

        // GET: api/event/status/{status}
        [HttpGet("status/{status}")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<EventDTO>>> GetEventsByStatus(EventStatus status)
        {
            var events = await _eventService.GetEventsByStatusAsync(status);
            return Ok(events);
        }

        // PUT: api/event/toggle-status/{id}/{status}
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

        // GET: api/event/upcoming
        [HttpGet("upcoming")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<EventDTO>>> GetUpcomingEvents()
        {
            var upcomingEvents = await _eventService.GetUpcomingEventsAsync();
            return Ok(upcomingEvents);
        }

        // GET: api/event/search/{keyword}
        [HttpGet("search/{keyword}")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<EventDTO>>> SearchEventsByTitle(string keyword)
        {
            var events = await _eventService.SearchEventsByTitleAsync(keyword);
            return Ok(events);
        }
    }
}
