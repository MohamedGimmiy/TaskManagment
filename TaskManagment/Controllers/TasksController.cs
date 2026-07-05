using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskManagment.Domain.Models;
using TaskManagment.Infrastructure.Repostories;
using TaskStatus = TaskManagment.Domain.Models.TaskStatus;

namespace TaskManagment.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TasksController : ControllerBase
    {
        private readonly TaskRepository _taskRepository;

        public TasksController(TaskRepository taskRepository)
        {
            _taskRepository = taskRepository;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTaskRequest request)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;

            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
            {
                return Unauthorized("User ID not found or invalid in token.");
            }

            var task = new TaskItem
            {
                Id = Guid.NewGuid(),
                Title = request.Title,
                Description = request.Description ?? string.Empty,
                Status = TaskStatus.Pending,
                Priority = request.Priority ?? "Medium",
                CreatedAt = DateTime.UtcNow,
                UserId = userId
            };

            await _taskRepository.Create(task);

            return Ok(new { task.Id, task.Title, task.Description, task.Status, task.Priority, task.CreatedAt });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;

            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
            {
                return Unauthorized("User ID not found or invalid in token.");
            }

            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

            var task = await _taskRepository.GetById(id);
            if (task == null)
            {
                return NotFound();
            }

            if (currentUserRole != "Admin" && task.UserId != userId)
            {
                return Forbid();
            }

            return Ok(new { task.Id, task.Title, task.Description, task.Status, task.Priority, task.CreatedAt });
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;

            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
            {
                return Unauthorized("User ID not found or invalid in token.");
            }

            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

            IEnumerable<TaskItem> tasks;
            if (currentUserRole == "Admin")
            {
                tasks = await _taskRepository.GetAll();
            }
            else
            {
                tasks = await _taskRepository.GetByUserId(userId);
            }

            return Ok(tasks.Select(t => new { t.Id, t.Title, t.Description, t.Status, t.Priority, t.CreatedAt }));
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateTaskStatusRequest request)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;

            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
            {
                return Unauthorized("User ID not found or invalid in token.");
            }

            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

            var task = await _taskRepository.GetById(id);
            if (task == null)
            {
                return NotFound();
            }

            if (currentUserRole != "Admin" && task.UserId != userId)
            {
                return Forbid();
            }

            if (!Enum.TryParse<TaskStatus>(request.Status, true, out var newStatus))
            {
                return BadRequest($"Invalid status. Valid values: {string.Join(", ", Enum.GetNames<TaskStatus>())}");
            }

            var updated = await _taskRepository.UpdateStatus(id, newStatus);
            if (updated == null)
            {
                return NotFound();
            }

            return Ok(new { updated.Id, updated.Title, updated.Description, updated.Status, updated.Priority, updated.CreatedAt });
        }
    }

    public class CreateTaskRequest
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Priority { get; set; }
    }

    public class UpdateTaskStatusRequest
    {
        public string Status { get; set; } = string.Empty;
    }
}
