using EventPlanner.DTOs;
using EventPlanner.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EventPlanner.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
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
            var user = await _userManager.FindByEmailAsync(model.EmailOrUsername) ??
                       await _userManager.FindByNameAsync(model.EmailOrUsername);

            if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
                return Unauthorized(new { message = "Invalid login attempt." });

            // Get user roles
            var userRoles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Sub, user.Id)
                };

            // Add role claims
            foreach (var role in userRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(3),
                signingCredentials: creds
            );

            return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) });
        }


        [HttpPost("role")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateRole([FromBody] string roleName)
        {
            if (string.IsNullOrEmpty(roleName))
                return BadRequest("Role name cannot be empty");

            if (await _roleManager.RoleExistsAsync(roleName))
                return Conflict($"Role '{roleName}' already exists");

            var result = await _roleManager.CreateAsync(new IdentityRole(roleName));

            return result.Succeeded ?
                Ok($"Role '{roleName}' created") :
                StatusCode(500, result.Errors);
        }


        [Authorize(Roles = "Admin")]
        [HttpPost("assign-role")]
        public async Task<IActionResult> AssignRoleToUser([FromBody] UserRoleAssignmentDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.UserId) || string.IsNullOrWhiteSpace(dto.RoleName))
            {
                return BadRequest("UserId and RoleName are required.");
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


        [Authorize]
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            return Ok(new { message = "Logged out successfully" });
        }
    }
}


