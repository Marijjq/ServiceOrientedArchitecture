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
        private readonly IUserService _userService;

        public RsvpController(IRsvpService rsvpService, IUserService userService)
        {
            _rsvpService = rsvpService;
            _userService = userService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Organizer")]
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

            var currentUserId = User.FindFirst("id")?.Value;
            if (currentUserId == null)
                return Unauthorized();

            rsvpDto.UserId = currentUserId;

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
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
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
            catch (Exception ex)
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
            var currentUserId = User.FindFirst("id")?.Value;
            if (currentUserId == null)
                return Unauthorized();

            try
            {
                var rsvp = await _rsvpService.GetRsvpByIdAsync(id);
                if (rsvp == null)
                    return NotFound(new { message = "RSVP not found." });

                if (rsvp.UserId != currentUserId && !await _userService.IsAdminAsync(currentUserId))
                    return Forbid();

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
            var currentUserId = User.FindFirst("id")?.Value;
            if (userId.ToString() != currentUserId && !User.IsInRole("Admin") && !User.IsInRole("Organizer"))
                return Forbid();

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
