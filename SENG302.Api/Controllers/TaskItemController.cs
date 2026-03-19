using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SENG302.Api.Filters;
using SENG302.Api.Models.Entities;
using SENG302.Api.Services;
using System.Security.Claims;
namespace SENG302.Api.Controllers;

[ConditionalValidateAntiForgeryToken]
[Authorize]
[ApiController]
[Route("api/taskItem")]
public class TaskItemController : ControllerBase
{
    private readonly ITaskService _taskService;

    public TaskItemController(ITaskService taskService)
    {
        _taskService = taskService;
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
        var taskList = await _taskService.GetTaskItemsByListAsync(listId);
        Console.Write(taskList);
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
    public async Task<ActionResult<TaskItem>> CreateTaskItem([FromBody] NewTaskItemRequest taskItem)
    {
        try
        {
            if (string.IsNullOrEmpty(taskItem.Name) || taskItem.Name.Length < 3 || taskItem.Name.Length > 128)
            {
                return BadRequest("Title is required and must be between 3 and 128 characters long");
            }
            if (taskItem.Description.Length > 2048)
            {
                return BadRequest("Description name cannot be more than 2048 characters long.");
            }
            
            if (taskItem.TaskListId == -1 || await _taskService.GetTaskListByIdAsync(taskItem.TaskListId) == null) // -1 is assigned to taskListId if nothing was provided
            {
                return BadRequest("Every task needs a list! Create one first.");
            }
            if (taskItem.CurrentStatus != CurrentTaskStatus.Done && 
                taskItem.CurrentStatus != CurrentTaskStatus.InProgress && 
                taskItem.CurrentStatus != CurrentTaskStatus.Todo) {
                return BadRequest("Not a valid status!");
            }

            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var taskList = await _taskService.GetTaskListByIdAsync(taskItem.TaskListId);
            if (taskList.UserEmail == userEmail) {
                await _taskService.CreateNewTaskItemAsync(taskItem);
                return Ok("Task created successfully");
            }
            else if (taskList.UserEmail != userEmail) {
                return Unauthorized("You are not authorised to add tasks to that list!");
            }
            else {
                return BadRequest("List does not exist.");
            }
        }
        catch (ArgumentException e)
        {
            return BadRequest(e);
        }
    }
}