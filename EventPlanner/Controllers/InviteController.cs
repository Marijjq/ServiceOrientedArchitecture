using EventPlanner.DTOs.Invite;
using EventPlanner.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EventPlanner.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class InviteController : ControllerBase
    {
        private readonly IInviteService _inviteService;

        public InviteController(IInviteService inviteService)
        {
            _inviteService = inviteService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Organizer")]
        public async Task<IActionResult> GetAllInvites()
        {
            var invites = await _inviteService.GetAllInvitesAsync();
            return Ok(invites);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetInviteById(int id)
        {
            var invite = await _inviteService.GetInviteByIdAsync(id);
            if (invite == null)
                return NotFound("Invite not found");

            return Ok(invite);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Organizer")]
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

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Organizer")]
        public async Task<IActionResult> UpdateInvite(int id, [FromBody] InviteUpdateDTO inviteDto)
        {
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

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
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

        [HttpGet("user/{userId}/pending")]
        public async Task<IActionResult> GetPendingInvitesByUserId(string userId)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var isAdmin = User.IsInRole("Admin");

            // Only Admins can view other users' invites
            if (!isAdmin && userId != currentUserId)
                return StatusCode(403, "You can only view your own invitations.");

            var invites = await _inviteService.GetPendingInvitesByUserIdAsync(userId);
            return Ok(invites);
        }

    }
}
