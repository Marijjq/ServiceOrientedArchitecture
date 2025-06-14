using EventPlanner.DTOs.User;
using EventPlanner.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace EventPlanner.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
   // [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
       // private readonly IMemoryCache _cache;
        //private const string AllProductsCacheKey = "AllUsersCache";

        public UserController(IUserService userService /*IMemoryCache cache*/)
        {
            _userService = userService;
            //_cache = cache;
        }

        // GET: api/user
        [HttpGet]
        // [Authorize(Roles = "Admin")]
        [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Client)]
        public async Task<ActionResult<IEnumerable<UserDTO>>> GetAllUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }

        // GET: api/user/{id}
       [HttpGet("{id}")]
        public async Task<ActionResult<UserDTO>> GetUserById(string id)
        {
            var currentUserId = User.FindFirst("id")?.Value;
            if (id != currentUserId && !User.IsInRole("Admin"))
                return Forbid();

            var user = await _userService.GetUserByIdAsync(id);
            if (user == null) return NotFound("User not found.");
            return Ok(user);

        }

        // POST: api/user
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<UserDTO>> CreateUser([FromBody] UserCreateDTO user)
        {
            if (user == null)
                return BadRequest("User cannot be null.");
            var createdUser = await _userService.CreateUserAsync(user);
            return CreatedAtAction(nameof(GetUserById), new { id = createdUser.Id }, createdUser);
        }

        // PUT: api/user/{id}
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] UserUpdateDTO userDto)
        {
            var currentUserId = User.FindFirst("id")?.Value;
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


        // DELETE: api/user/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            //if (!User.IsInRole("Admin"))
            //    return Forbid();

            await _userService.DeleteUserAsync(id);
            return NoContent();
        }

        // GET: api/user/email/{email}
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
