using Microsoft.AspNetCore.Mvc;
using SENG302.Api;

namespace SENG302.Api.Controllers;

[ApiController]
public class TaskController : ControllerBase
{
    private readonly ITaskService _taskService;
}