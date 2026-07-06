using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagment.Domain.ServicesContract;

namespace TaskManagment.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateUserRequest request)
        {
            try
            {
                var user = await _userService.CreateUser(request.Name, request.Email, request.Password, request.Role);
                return Ok(new { user.Id, user.Name, user.Email, user.Role, user.CreatedAt });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll()
        {
            var users = await _userService.GetAllUsers();
            return Ok(users.Select(u => new { u.Id, u.Name, u.Email, u.Role, u.CreatedAt }));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var currentUserId = Guid.Parse(User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value ?? string.Empty);
            var currentUserRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

            var user = await _userService.GetUserById(id, currentUserId, currentUserRole ?? "");

            if (user == null)
            {
                if (await _userService.GetUserById(id, Guid.Empty, "Admin") != null)
                {
                    return Forbid();
                }
                return NotFound();
            }

            return Ok(new { user.Id, user.Name, user.Email, user.Role, user.CreatedAt });
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userIdString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;

            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var currentUserId))
            {
                return Unauthorized("User ID not found or invalid in token.");
            }

            var user = await _userService.GetProfile(currentUserId);

            if (user == null)
            {
                return NotFound();
            }

            return Ok(new { user.Id, user.Name, user.Email, user.Role, user.CreatedAt });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var user = await _userService.GetUserById(id, Guid.Empty, "Admin");
            if (user == null)
            {
                return NotFound();
            }

            await _userService.DeleteUser(id);
            return NoContent();
        }
    }

    public class CreateUserRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = "User";
    }

    public class UpdateUserRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Password { get; set; }
    }
}
