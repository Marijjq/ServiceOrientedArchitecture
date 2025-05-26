using EventPlanner.DTOs.Invite;
using EventPlanner.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EventPlanner.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InviteController : ControllerBase
    {
        private readonly IInviteService _inviteService;

        public InviteController(IInviteService inviteService)
        {
            _inviteService = inviteService;
        }

        // Get all invites
        [HttpGet]
        public async Task<IActionResult> GetAllInvites()
        {
            var invites = await _inviteService.GetAllInvitesAsync();
            return Ok(invites);
        }

        // Get invite by ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetInviteById(int id)
        {
            var invite = await _inviteService.GetInviteByIdAsync(id);
            if (invite == null)
                return NotFound("Invite not found");

            return Ok(invite);
        }

        // Send (create) an invite
        [HttpPost]
        public async Task<IActionResult> SendInvite([FromBody] InviteCreateDTO inviteDto)
        {
            try
            {
                var createdInvite = await _inviteService.SendInviteAsync(inviteDto);
                return CreatedAtAction(nameof(GetInviteById), new { id = createdInvite.Id }, createdInvite);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Update an invite
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateInvite(int id, [FromBody] InviteUpdateDTO inviteDto)
        {
            // Removed the ID check since InviteUpdateDTO does not have an Id property
            try
            {
                var updatedInvite = await _inviteService.UpdateInviteAsync(id, inviteDto);
                return Ok(updatedInvite);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Delete an invite
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInvite(int id)
        {
            try
            {
                await _inviteService.DeleteInviteAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Get pending invites for a user
        [HttpGet("user/{userId}/pending")]
        public async Task<IActionResult> GetPendingInvitesByUserId(int userId)
        {
            var invites = await _inviteService.GetPendingInvitesByUserIdAsync(userId);
            return Ok(invites);
        }

    }
}
