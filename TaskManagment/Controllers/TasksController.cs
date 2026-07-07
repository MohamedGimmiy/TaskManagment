using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskManagment.Domain.ServicesContract;
using TaskStatus = TaskManagment.Domain.Models.TaskStatus;

namespace TaskManagment.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TasksController : ControllerBase
    {
        private readonly ITaskService _taskService;
        private readonly ITaskProcessingQueue _taskQueue;

        public TasksController(ITaskService taskService, ITaskProcessingQueue taskQueue)
        {
            _taskService = taskService;
            _taskQueue = taskQueue;
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

            try
            {
                var task = await _taskService.CreateTask(userId, request.Title, request.Description, request.Priority);
                _taskQueue.EnqueueTask(task.Id);

                return Ok(new { task.Id, task.Title, task.Description, task.Status, task.Priority, task.CreatedAt });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
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
            var task = await _taskService.GetTaskById(id, userId, currentUserRole ?? "");

            if (task == null)
            {
                var exists = await _taskService.GetTaskById(id, Guid.Empty, "Admin");
                if (exists != null)
                {
                    return Forbid();
                }
                return NotFound();
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
            var tasks = await _taskService.GetUserTasks(userId, currentUserRole ?? "");

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

            if (!Enum.TryParse<TaskStatus>(request.Status, true, out var newStatus))
            {
                return BadRequest($"Invalid status. Valid values: {string.Join(", ", Enum.GetNames<TaskStatus>())}");
            }

            var updated = await _taskService.UpdateTaskStatus(id, newStatus, userId, currentUserRole ?? "");

            if (updated == null)
            {
                var exists = await _taskService.GetTaskById(id, Guid.Empty, "Admin");
                if (exists != null)
                {
                    return Forbid();
                }
                return NotFound();
            }

            return Ok(new { updated.Id, updated.Title, updated.Description, updated.Status, updated.Priority, updated.CreatedAt });
        }
    }

    public class CreateTaskRequest
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Priority { get; set; } = 2;
    }

    public class UpdateTaskStatusRequest
    {
        public string Status { get; set; } = string.Empty;
    }
}
