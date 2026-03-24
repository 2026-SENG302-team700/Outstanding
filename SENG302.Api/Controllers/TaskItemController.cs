using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SENG302.Api.Filters;
using SENG302.Api.Models.Entities;
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
            var response = await _taskItemService.CreateNewTaskItemAsync(taskItemRequest);
            return Ok(response);
        }
        catch (InvalidLengthException e)
        {
            return BadRequest(e.Message);
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
            var response = await _taskItemService.GetTaskItemAsync(id);
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
            var taskItem = await _taskItemService.UpdateTaskItemAsync(taskItemUpdates);
            return Ok(taskItem);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }
}