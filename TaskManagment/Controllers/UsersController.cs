using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagment.Domain.Models;
using TaskManagment.Infrastructure.Repostories;

namespace TaskManagment.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly UserRepository _userRepository;

        public UsersController(UserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll()
        {
            var users = await _userRepository.GetAll();
            return Ok(users.Select(u => new { u.Id, u.Name, u.Email, u.Role, u.CreatedAt }));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var user = await _userRepository.GetById(id);
            if (user == null)
            {
                return NotFound();
            }

            // Users can only view their own profile unless they are admin
            var currentUserId = Guid.Parse(User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value ?? string.Empty);
            var currentUserRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

            if (currentUserRole != "Admin" && currentUserId != id)
            {
                return Forbid();
            }

            return Ok(new { user.Id, user.Name, user.Email, user.Role, user.CreatedAt });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserRequest request)
        {
            var user = await _userRepository.GetById(id);
            if (user == null)
            {
                return NotFound();
            }

            // Users can only update their own profile unless they are admin
            var currentUserId = Guid.Parse(User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value ?? string.Empty);
            var currentUserRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

            if (currentUserRole != "Admin" && currentUserId != id)
            {
                return Forbid();
            }

            user.Name = request.Name;
            user.Email = request.Email;

            if (!string.IsNullOrEmpty(request.Password))
            {
                user.Password = BCrypt.Net.BCrypt.HashPassword(request.Password);
            }

            var updatedUser = await _userRepository.Update(user);
            return Ok(new { updatedUser.Id, updatedUser.Name, updatedUser.Email, updatedUser.Role, updatedUser.CreatedAt });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var user = await _userRepository.GetById(id);
            if (user == null)
            {
                return NotFound();
            }

            await _userRepository.Delete(id);
            return NoContent();
        }
    }

    public class UpdateUserRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Password { get; set; }
    }
}
