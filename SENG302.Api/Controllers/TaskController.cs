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
    public async Task<ActionResult<TaskList>> getTaskList(int id)
    {
        var taskList = await _taskService.GetTaskListByIdAsync(id);
        if (taskList == null)
        {
            return NotFound();
        }
        return Ok(taskList);
    }

    [HttpGet("user/{userEmail}")]
    public async Task<ActionResult<IEnumerable<TaskList>>> GetTaskListsByUser(string userEmail)
    {
        var taskLists = await _taskService.GetTaskListsByUserEmailAsync(userEmail);
        return Ok(taskLists);
    }

    [HttpPost]
    public async Task<ActionResult<TaskList>> CreateTaskList([FromBody] NewTaskListRequest taskListRequest)
    {
        try
        {
            await _taskService.CreateNewTaskListAsync(taskListRequest.Name, taskListRequest.UserEmail);
            return Ok("Task list created successfully");
        }
        catch (ArgumentException e)
        {
            return BadRequest(e.Message);
        }
    }
}