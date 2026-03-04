using Microsoft.AspNetCore.Mvc;
using SENG302.Api.Services;
using SENG302.Api.Models.Entities;
using SENG302.Api.Models.Requests;
using SENG302.Api.Filters;
using System.Security.Claims;
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
namespace SENG302.Api.Controllers;

[ConditionalValidateAntiForgeryToken]
[Authorize]
[ApiController]
[Route("api/tasks")]
public class TaskController : ControllerBase
{
    private readonly ITaskService _taskService;

    public TaskController(ITaskService taskService)
    {
        _taskService = taskService;
    }

    /// <summary>
    /// Gets a task list by its ID. Returns 404 if no task list with the given ID exists.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<TaskList>> getTaskList(int id)
    {
        var taskList = await _taskService.GetTaskListByIdAsync(id);
        if (taskList == null)
        {
            return NotFound();
        }
        return Ok(taskList);
    }

    /// <summary>
    /// Gets all task lists associated with a user's email. Returns an empty array if no task lists are found for the given email.
    /// </summary>
    /// <param name="userEmail"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TaskList>>> GetTaskListsForUser()
    {
        var userEmail = User.FindFirstValue(ClaimTypes.Email);
        if (string.IsNullOrEmpty(userEmail))
            return Unauthorized();


        var taskLists = await _taskService.GetTaskListsByUserEmailAsync(userEmail);
        return Ok(taskLists);
    }

    /// <summary>
    /// Creates a new task list for a user with the given email and name. 
    /// Validates the name and user email before creating the task list.
    /// Returns 400 if validation fails, or 200 if the task list is created successfully.
    /// </summary>
    /// <param name="taskListRequest"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ActionResult<TaskList>> CreateTaskList([FromBody] NewTaskListRequest taskListRequest)
    {
        try
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(userEmail))
                return Unauthorized();
            await _taskService.CreateNewTaskListAsync(taskListRequest.Name, userEmail);
            return Ok("Task list created successfully");
        }
        catch (ArgumentException e)
        {
            return BadRequest(e.Message);
        }
    }
}