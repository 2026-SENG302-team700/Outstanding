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
    public async Task<ActionResult<TaskItem>> CreateTaskItem(TaskItem taskItem)
    {

    }
}