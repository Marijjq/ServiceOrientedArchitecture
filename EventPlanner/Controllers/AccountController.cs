using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using EventPlanner.DTOs;
using EventPlanner.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EventPlanner.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AccountController(UserManager<ApplicationUser> userManager,
                                 IConfiguration configuration,
                                 RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _configuration = configuration;
            _roleManager = roleManager;
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModelDTO model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userExistsByEmail = await _userManager.FindByEmailAsync(model.Email);
            if (userExistsByEmail != null)
            {
                return Conflict(new { message = "User with this email already exists." });
            }

            var userExistsByUsername = await _userManager.FindByNameAsync(model.Username);
            if (userExistsByUsername != null)
            {
                return Conflict(new { message = "User with this username already exists." });
            }

            var user = new ApplicationUser
            {
                UserName = model.Username,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                PhoneNumber = model.PhoneNumber
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "User creation failed.", errors = result.Errors.Select(e => e.Description) });
            }

            // Optionally assign default role "User"
            await _userManager.AddToRoleAsync(user, "User");

            return Ok(new { message = "User registered successfully!" });
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModelDTO model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            ApplicationUser user = null;

            if (model.EmailOrUsername.Contains("@"))
            {
                user = await _userManager.FindByEmailAsync(model.EmailOrUsername);
            }
            else
            {
                user = await _userManager.FindByNameAsync(model.EmailOrUsername);
            }

            if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
            {
                return Unauthorized(new { message = "Invalid login attempt." });
            }

            var userRoles = await _userManager.GetRolesAsync(user);

            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.Id),  // <-- Added user ID claim here
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            foreach (var role in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, role));
            }

            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                expires: DateTime.Now.AddMinutes(30),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                expiration = token.ValidTo
            });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("role")]
        public async Task<IActionResult> CreateRole([FromBody] string roleName)
        {
            if (string.IsNullOrEmpty(roleName))
            {
                return BadRequest("Role name cannot be empty.");
            }

            var roleExists = await _roleManager.RoleExistsAsync(roleName);
            if (roleExists)
            {
                return Conflict(new { message = $"Role '{roleName}' already exists." });
            }

            var result = await _roleManager.CreateAsync(new IdentityRole(roleName));
            if (result.Succeeded)
            {
                return Ok(new { message = $"Role '{roleName}' created successfully." });
            }

            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Role creation failed.", errors = result.Errors.Select(e => e.Description) });
        }
        [Authorize(Roles = "Admin")]
        [HttpPost("assign-role")]
        public async Task<IActionResult> AssignRoleToUser([FromBody] UserRoleAssignmentDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.UserId) || string.IsNullOrWhiteSpace(dto.RoleName))
            {
                return BadRequest("UserId and RoleName are required.");
            }

            // Extra safety: prevent even Admins from assigning the Admin role unless truly authorized
            if (dto.RoleName == "Admin")
            {
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var currentUser = await _userManager.FindByIdAsync(currentUserId);

                if (currentUser == null || !await _userManager.IsInRoleAsync(currentUser, "Admin"))
                {
                    return Forbid("Only admins can assign the Admin role.");
                }
            }

            var user = await _userManager.FindByIdAsync(dto.UserId);
            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            var roleExists = await _roleManager.RoleExistsAsync(dto.RoleName);
            if (!roleExists)
            {
                return NotFound(new { message = "Role does not exist." });
            }

            var result = await _userManager.AddToRoleAsync(user, dto.RoleName);
            if (result.Succeeded)
            {
                return Ok(new { message = $"Role '{dto.RoleName}' assigned to user '{user.UserName}' successfully." });
            }

            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                message = "Failed to assign role.",
                errors = result.Errors.Select(e => e.Description)
            });
        }

    }
}
