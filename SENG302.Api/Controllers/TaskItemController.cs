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
        return Ok(taskList);
    }

    [HttpPost]
    public async Task<ActionResult<TaskItem>> CreateTaskItem([FromBody] NewTaskItemRequest taskItem)
    {
        try
        {
            if (taskItem == null)
            {
                return BadRequest("One or more fields are missing!");
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