using Microsoft.AspNetCore.Mvc;
using SENG302.Api.Services;
using SENG302.Api.Models.Entities;
using SENG302.Api.Models.Requests;

namespace SENG302.Api.Controllers;

[ApiController]
[Route("api/tasks")]
public class TaskController : ControllerBase
{
    private readonly ITaskService _taskService;

    public TaskController(ITaskService taskService)
    {
        _taskService = taskService;
    }

    [HttpGet("{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult<TaskList>> getTaskList(int id)
    {
        var taskList = await _taskService.GetTaskListByIdAsync(id);
        if (taskList == null)
        {
            return NotFound();
        }
        return Ok(taskList);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult<TaskList>> CreateTaskList([FromBody] NewTaskListRequest taskListRequest)
    {
        try
        {
            var newTaskList = await _taskService.CreateNewTaskListAsync(taskListRequest.Name, taskListRequest.UserEmail);
            return CreatedAtAction(nameof(getTaskList), new { id = newTaskList.Id }, newTaskList);
        }
        catch (ArgumentException e)
        {
            return BadRequest(e.Message);
        }
    }
}