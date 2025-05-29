using EventPlanner.DTOs.RSVP;
using EventPlanner.Enums;
using EventPlanner.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventPlanner.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class RsvpController : ControllerBase
    {
        private readonly IRsvpService _rsvpService;

        public RsvpController(IRsvpService rsvpService)
        {
            _rsvpService = rsvpService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllRsvps()
        {
            var rsvps = await _rsvpService.GetAllRsvpsAsync();
            return Ok(rsvps);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetRsvpById(int id)
        {
            try
            {
                var rsvp = await _rsvpService.GetRsvpByIdAsync(id);
                return Ok(rsvp);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] RsvpCreateDTO rsvpDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var created = await _rsvpService.CreateRsvpAsync(rsvpDto);
                return CreatedAtAction(nameof(GetRsvpById), new { id = created.Id }, created);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] RsvpUpdateDTO rsvpDto)
        {
            try
            {
                var updated = await _rsvpService.UpdateRsvpAsync(id, rsvpDto);
                return Ok(updated);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Organizer")]

        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _rsvpService.DeleteRsvpAsync(id);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromQuery] RSVPStatus status)
        {
            try
            {
                var result = await _rsvpService.UpdateStatusAsync(id, status);
                return Ok(new { success = result });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("event/{eventId}")]
        public async Task<IActionResult> GetByEvent(int eventId)
        {
            var result = await _rsvpService.GetRsvpsByEventIdAsync(eventId);
            return Ok(result);
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUser(int userId)
        {
            var result = await _rsvpService.GetRsvpsByUserIdAsync(userId);
            return Ok(result);
        }

        [HttpPatch("{id}/cancel")]
        public async Task<IActionResult> Cancel(int id)
        {
            try
            {
                var result = await _rsvpService.CancelRsvpAsync(id);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // New endpoint: Respond to invite
        [HttpPost("invite/{inviteId}/respond")]
        public async Task<IActionResult> RespondToInvite(int inviteId, [FromQuery] RSVPStatus status)
        {
            try
            {
                var response = await _rsvpService.RespondToInviteAsync(inviteId, status);
                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
