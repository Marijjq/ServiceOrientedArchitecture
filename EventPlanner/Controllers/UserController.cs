using EventPlanner.DTOs.User;
using EventPlanner.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Claims;

namespace EventPlanner.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
       private readonly IMemoryCache _cache;
      private const string AllProductsCacheKey = "AllUsersCache";

        public UserController(IUserService userService, IMemoryCache cache)
        {
            _userService = userService;
            _cache = cache;
        }

        [HttpGet]
        // [Authorize(Roles = "Admin")]
        [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Client)]
        public async Task<ActionResult<IEnumerable<UserDTO>>> GetAllUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<UserDTO>> GetUserById(string id)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // SIMPLIFIED: Built-in role check
            var isAdmin = User.IsInRole("Admin");

            if (id != currentUserId && !isAdmin)
                return StatusCode(403, "You are not authorized to view this user's info.");

            var user = await _userService.GetUserByIdAsync(id);
            if (user == null) return NotFound("User not found.");
            return Ok(user);
        }


        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<UserDTO>> CreateUser([FromBody] UserCreateDTO user)
        {
            if (user == null)
                return BadRequest("User cannot be null.");
            var createdUser = await _userService.CreateUserAsync(user);
            return CreatedAtAction(nameof(GetUserById), new { id = createdUser.Id }, createdUser);
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] UserUpdateDTO userDto)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value; 
            var isAdmin = User.IsInRole("Admin");

            if (id != currentUserId && !isAdmin)
                return Forbid("Only the user or an admin can update this information.");

            try
            {
                await _userService.UpdateUserAsync(id, userDto, currentUserId);
                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
        }


        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            //if (!User.IsInRole("Admin"))
            //    return Forbid();

            await _userService.DeleteUserAsync(id);
            return NoContent();
        }

        [HttpGet("email/{email}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<UserDTO>> GetUserByEmail(string email)
        {
            var user = await _userService.GetUserByEmailAsync(email);
            if (user == null)
                return NotFound("User not found.");
            return Ok(user);
        }
    }
}
