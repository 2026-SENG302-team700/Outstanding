using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SENG302.Api.Filters;
using SENG302.Api.Models.Entities;
using SENG302.Api.Models.Requests;
using SENG302.Api.Services;
namespace SENG302.Api.Controllers;

[ConditionalValidateAntiForgeryToken]
[Authorize]
[ApiController]
[Route("api/taskItem")]
public class TaskItemController : ControllerBase
{
    private readonly ITaskItemService _taskItemService;

    public TaskItemController(ITaskItemService taskItemService)
    {
        _taskItemService = taskItemService;
    }

    /// <summary>
    /// Fetchs all the task items for the current user
    /// </summary>
    /// <returns>All tasks belonging to that user</returns>
    [Authorize]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TaskItem>>> GetAllTasks()
    {
        // get the logged in user id
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString))
        {
            return Unauthorized();
        }

        try
        {
            var tasks = await _taskItemService.GetAllTaskItemsAsync(int.Parse(userIdString));
            return Ok(tasks);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    /// <summary>
    /// Fetches all task items associated to the id of the given list. If
    /// no list is provided, then throw a BadRequest Error.
    /// </summary>
    /// <param name="listId = -1"></param>
    /// <returns>The list of tasks</returns>
    [HttpGet("{listId:int}")]
    public async Task<ActionResult<IEnumerable<TaskItem>>> GetTasksFromList(int listId = -1)
    {
        if (listId < 0)
        {
            return BadRequest("List not provided");
        }
        var taskList = await _taskItemService.GetTaskItemsByListAsync(listId);
        return Ok(taskList);
    }

    /// <summary>
    /// Given a task item request object, create a new task item and add it to the db
    /// Fails if: the task name is too short (characters) or long 128 (characters),
    /// the description is longer than 2048 characters, the task list doesn't exist,
    /// the status is not valid or the user is not authorised.
    /// </summary>
    /// <param name="taskItem"></param>
    /// <returns>The list of tasks</returns>
    [HttpPost]
    public async Task<ActionResult<TaskItem>> CreateTaskItem([FromBody] NewTaskItemRequest taskItemRequest)
    {
        try
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var response = await _taskItemService.CreateNewTaskItemAsync(taskItemRequest, userId);
            return Ok(response);
        }
        catch (MultipleValidationException e)
        {
            return BadRequest(new BadRequestValidationResponse
            {
                Errors = e.Errors
            });
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpGet("item/{id:int}")]
    public async Task<ActionResult<TaskItem>> GetTaskItem(int id)
    {
        try
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var response = await _taskItemService.GetTaskItemAsync(id, userId);
            return Ok(response);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    /// <summary>
    /// Given a request to update a task, it will send a request to update that task
    /// to the TaskItem Service
    /// </summary>
    /// <param name="taskItemUpdates">updated values for a task item</param>
    /// <returns>
    /// The updated task item if ok
    /// A Bad Request if an error occured within (likely validation fail).
    /// </returns>
    [HttpPut("item/{id:int}")]
    public async Task<ActionResult<TaskItem>> UpdateTaskItem([FromBody] UpdateTaskItemRequest taskItemUpdates)
    {
        try
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var taskItem = await _taskItemService.UpdateTaskItemAsync(taskItemUpdates, userId);
            return Ok(taskItem);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }
}