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

        // ✅ Admin/Organizer: Get all invites
        [HttpGet]
        [Authorize(Roles = "Admin,Organizer")]
        public async Task<IActionResult> GetAllInvites()
        {
            var invites = await _inviteService.GetAllInvitesAsync();
            return Ok(invites);
        }

        // ✅ Any authorized user: Get a single invite by ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetInviteById(int id)
        {
            var invite = await _inviteService.GetInviteByIdAsync(id);
            if (invite == null)
                return NotFound("Invite not found");

            return Ok(invite);
        }

        // ✅ Admin/Organizer: Send an invite
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

        // ✅ Admin/Organizer: Update an invite
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

        // ✅ Admin only: Delete invite
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

        // ✅ Get pending invites (user can only see their own unless Admin or Organizer)
        [HttpGet("user/{userId}/pending")]
        public async Task<IActionResult> GetPendingInvitesByUserId(string userId)
        {
            var currentUserId = User.FindFirst("id")?.Value;
            var isAdmin = User.IsInRole("Admin");
            var isOrganizer = User.IsInRole("Organizer");

            if (currentUserId != userId && !isAdmin && !isOrganizer)
                return Forbid();

            var invites = await _inviteService.GetPendingInvitesByUserIdAsync(userId);
            return Ok(invites);
        }
    }
}
